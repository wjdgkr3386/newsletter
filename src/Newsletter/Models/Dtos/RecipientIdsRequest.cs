namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/recipients, POST /api/recipients/delete 요청
/// </summary>
public record RecipientIdsRequest(List<long>? UserIds);
