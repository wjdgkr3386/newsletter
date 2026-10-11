using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newsletter.Models.Dtos;
using Newsletter.Services;

namespace Newsletter.Controllers.Api;

/// <summary>
/// 뉴스레터 설정 API (api.md 6.3). 관리자만
/// </summary>
[Route("api/newsletter-settings")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public class NewsletterSettingsController : ApiControllerBase
{
    private readonly INewsletterSettingService _settingService;

    public NewsletterSettingsController(INewsletterSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAsync(CancellationToken cancellationToken)
    {
        NewsletterSettingResponse setting = await _settingService.GetAsync(cancellationToken);
        return ToActionResult(ServiceResult<NewsletterSettingResponse>.Success(setting));
    }

    [HttpPost]
    public async Task<ActionResult> SaveAsync([FromBody] NewsletterSettingRequest request, CancellationToken cancellationToken)
    {
        ServiceResult<NewsletterSettingResponse> result = await _settingService.SaveAsync(request.GenerateTime, request.SendTime, cancellationToken);
        return ToActionResult(result);
    }
}
