namespace Newsletter.Models.Entities;

/// <summary>
/// email_verifications 테이블 (이메일 인증 코드)
/// </summary>
public class EmailVerification
{
    public long Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int FailCount { get; set; }
}
