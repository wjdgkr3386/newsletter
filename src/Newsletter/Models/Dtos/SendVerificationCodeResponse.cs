namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/email-verifications 응답 data
/// </summary>
public record SendVerificationCodeResponse(DateTime ExpiresAt);
