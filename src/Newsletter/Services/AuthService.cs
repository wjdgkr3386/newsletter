using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Models.Entities;

namespace Newsletter.Services;

/// <summary>
/// 로그인 아이디·비밀번호 확인 (security.md 2: 무엇이 틀렸는지 알리지 않는다)
/// </summary>
public class AuthService : IAuthService
{
    private const string LoginFailedMessage = "아이디 또는 비밀번호가 맞지 않습니다.";

    // 없는 아이디도 비밀번호 확인과 같은 시간이 걸리게 해서, 응답 시간으로 아이디 존재 여부를 알 수 없게 한다
    private static readonly string DummyPasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy-password-for-timing");

    private readonly NewsletterDbContext _dbContext;

    public AuthService(NewsletterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<User>> ValidateCredentialsAsync(string? loginId, string? password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(loginId) || string.IsNullOrEmpty(password))
        {
            return ServiceResult<User>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput, "아이디와 비밀번호를 입력해 주세요.");
        }

        // 형식 규칙은 알려주지 않는다. 컬럼보다 긴 값은 DB를 조회하지 않고 실패로 처리한다
        User? user = loginId.Length <= InputValidator.LoginIdMaxLength && password.Length <= InputValidator.PasswordMaxLength
            ? await _dbContext.Users.FirstOrDefaultAsync(candidate => candidate.LoginId == loginId, cancellationToken)
            : null;

        bool isPasswordMatched = BCrypt.Net.BCrypt.Verify(password, user?.PasswordHash ?? DummyPasswordHash);
        if (user is null || !isPasswordMatched)
        {
            return ServiceResult<User>.Failure(StatusCodes.Status401Unauthorized, ErrorCodes.LoginFailed, LoginFailedMessage);
        }

        return ServiceResult<User>.Success(user);
    }
}
