namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/email-verifications 요청
/// </summary>
public record SendVerificationCodeRequest(string? Email);
