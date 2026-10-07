namespace Newsletter.Models.Dtos;

/// <summary>
/// api.md 공통 성공 응답 { "data": ... }
/// </summary>
public record ApiSuccessResponse<T>(T Data);
