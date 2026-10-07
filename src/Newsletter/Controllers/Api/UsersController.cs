using Microsoft.AspNetCore.Mvc;
using Newsletter.Models.Dtos;
using Newsletter.Services;

namespace Newsletter.Controllers.Api;

/// <summary>
/// 회원 API (api.md 6.1 회원가입)
/// </summary>
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult> RegisterAsync([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<RegisterUserResponse> result = await _userService.RegisterAsync(request, cancellationToken);
        return ToActionResult(result);
    }
}
