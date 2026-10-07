namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/email-verifications/confirm 응답 data
/// </summary>
public record ConfirmVerificationCodeResponse(bool IsVerified);
