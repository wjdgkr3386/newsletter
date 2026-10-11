using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;
using Newsletter.Services;

namespace Newsletter.Controllers.Api;

/// <summary>
/// 로그인, 로그아웃 API (api.md 6.1)
/// </summary>
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<User> result = await _authService.ValidateCredentialsAsync(request.LoginId, request.Password, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToActionResult(ServiceResult<LoginResponse>.Failure(result.Error!));
        }

        User user = result.Data!;
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.LoginId),
            new(ClaimTypes.Role, user.Role),
        ];
        ClaimsPrincipal principal = new(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        // 쿠키 만료(60분, 요청 시 연장)는 Program.cs 쿠키 설정을 따른다 (D-060)
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return ToActionResult(ServiceResult<LoginResponse>.Success(new LoginResponse(user.Role, HomePathResolver.GetHomePath(user.Role))));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return ToActionResult(ServiceResult<EmptyData>.Success(new EmptyData()));
    }
}
