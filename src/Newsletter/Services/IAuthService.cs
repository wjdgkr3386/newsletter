using Newsletter.Models.Entities;

namespace Newsletter.Services;

public interface IAuthService
{
    /// <summary>
    /// 아이디와 비밀번호를 확인한다. 성공하면 회원, 실패하면 LOGIN_FAILED 또는 INVALID_INPUT
    /// </summary>
    Task<ServiceResult<User>> ValidateCredentialsAsync(string? loginId, string? password, CancellationToken cancellationToken);
}
