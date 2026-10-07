namespace Newsletter.Models.Entities;

/// <summary>
/// recipients 테이블 (수신자)
/// </summary>
public class Recipient
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
}
