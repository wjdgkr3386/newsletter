using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Models.Dtos;
using Newsletter.Services;

namespace Newsletter.Controllers.Api;

/// <summary>
/// 회원 API (api.md 6.1 회원가입, 6.2 회원 목록)
/// </summary>
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;
    private readonly IRecipientService _recipientService;

    public UsersController(IUserService userService, IRecipientService recipientService)
    {
        _userService = userService;
        _recipientService = recipientService;
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public async Task<ActionResult> GetMembersAsync([FromQuery] bool? isRecipient, CancellationToken cancellationToken)
    {
        List<MemberResponse> members = await _recipientService.GetMembersAsync(isRecipient, cancellationToken);
        return ToActionResult(ServiceResult<List<MemberResponse>>.Success(members));
    }

    [HttpPost]
    public async Task<ActionResult> RegisterAsync([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<RegisterUserResponse> result = await _userService.RegisterAsync(request, cancellationToken);
        return ToActionResult(result);
    }
}
