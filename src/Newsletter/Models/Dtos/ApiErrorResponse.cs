namespace Newsletter.Models.Dtos;

/// <summary>
/// api.md 공통 실패 응답 { "error": { "code", "message" } }
/// </summary>
public record ApiErrorResponse(ApiErrorBody Error);
