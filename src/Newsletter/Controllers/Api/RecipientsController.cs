using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Models.Dtos;
using Newsletter.Services;

namespace Newsletter.Controllers.Api;

/// <summary>
/// 수신자 API (api.md 6.2). 관리자만
/// </summary>
[Route("api/recipients")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class RecipientsController : ApiControllerBase
{
    private readonly IRecipientService _recipientService;

    public RecipientsController(IRecipientService recipientService)
    {
        _recipientService = recipientService;
    }

    [HttpGet]
    public async Task<ActionResult> GetRecipientsAsync(CancellationToken cancellationToken)
    {
        List<RecipientResponse> recipients = await _recipientService.GetRecipientsAsync(cancellationToken);
        return ToActionResult(ServiceResult<List<RecipientResponse>>.Success(recipients));
    }

    [HttpPost]
    public async Task<ActionResult> AddAsync([FromBody] RecipientIdsRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<RecipientChangeResponse> result = await _recipientService.AddAsync(request.UserIds, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("delete")]
    public async Task<ActionResult> DeleteAsync([FromBody] RecipientIdsRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<RecipientChangeResponse> result = await _recipientService.DeleteAsync(request.UserIds, cancellationToken);
        return ToActionResult(result);
    }
}
