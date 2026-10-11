namespace Newsletter.Models.Dtos;

/// <summary>
/// GET, POST /api/newsletter-settings 응답 data. 시간은 HH:mm (KST)
/// </summary>
public record NewsletterSettingResponse(string GenerateTime, string SendTime);
