namespace Newsletter.Models.Dtos;

/// <summary>
/// POST /api/users 요청. 비밀번호 확인 값은 화면에서만 비교하므로 받지 않는다 (D-065)
/// </summary>
public record RegisterUserRequest(string? LoginId, string? Password, string? Email, string? Name);
