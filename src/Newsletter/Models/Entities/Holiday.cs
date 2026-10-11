namespace Newsletter.Models.Entities;

/// <summary>
/// holidays 테이블 (공휴일, 대체공휴일)
/// </summary>
public class Holiday
{
    public long Id { get; set; }

    public DateOnly HolidayDate { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
