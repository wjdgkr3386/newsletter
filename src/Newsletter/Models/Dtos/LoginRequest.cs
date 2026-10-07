namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/auth/login 요청
/// </summary>
public record LoginRequest(string? LoginId, string? Password);
