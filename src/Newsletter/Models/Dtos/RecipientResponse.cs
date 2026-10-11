namespace Newsletter.Models.Dtos;

/// <summary>
/// GET /api/recipients 응답 data의 한 행
/// </summary>
public record RecipientResponse(long UserId, string Name, string Email);
