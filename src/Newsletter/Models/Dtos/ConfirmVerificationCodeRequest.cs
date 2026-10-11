namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/email-verifications/confirm 요청
/// </summary>
public record ConfirmVerificationCodeRequest(string? Email, string? Code);
