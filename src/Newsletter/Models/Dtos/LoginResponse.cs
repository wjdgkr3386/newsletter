namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/auth/login 응답 data
/// </summary>
public record LoginResponse(string Role, string RedirectUrl);
