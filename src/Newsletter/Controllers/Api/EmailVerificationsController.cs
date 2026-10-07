using Microsoft.AspNetCore.Mvc;
using Newsletter.Models.Dtos;
using Newsletter.Services;

namespace Newsletter.Controllers.Api;

/// <summary>
/// 이메일 인증 코드 API (api.md 6.1)
/// </summary>
[Route("api/email-verifications")]
public class EmailVerificationsController : ApiControllerBase
{
    private readonly IEmailVerificationService _emailVerificationService;

    public EmailVerificationsController(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    [HttpPost]
    public async Task<ActionResult> SendCodeAsync([FromBody] SendVerificationCodeRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<SendVerificationCodeResponse> result = await _emailVerificationService.SendCodeAsync(request.Email, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult> ConfirmCodeAsync([FromBody] ConfirmVerificationCodeRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<ConfirmVerificationCodeResponse> result = await _emailVerificationService.ConfirmCodeAsync(request.Email, request.Code, cancellationToken);
        return ToActionResult(result);
    }
}
