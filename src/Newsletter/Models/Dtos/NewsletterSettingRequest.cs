namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/newsletter-settings 요청. 시간은 HH:mm (00:00~23:59, KST)
/// </summary>
public record NewsletterSettingRequest(string? GenerateTime, string? SendTime);
