namespace Newsletter.Models.Dtos;

/// <summary>
/// GET /api/users 응답 data의 한 행
/// </summary>
public record MemberResponse(long Id, string Name, string Email, bool IsRecipient);
