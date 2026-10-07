namespace Newsletter.Models.Entities;

/// <summary>
/// users 테이블 (회원)
/// </summary>
public class User
{
    public long Id { get; set; }

    public string LoginId { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
