namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/recipients, POST /api/recipients/delete 응답 data
/// </summary>
public record RecipientChangeResponse(int Count);
