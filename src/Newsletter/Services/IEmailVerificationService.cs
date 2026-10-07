using Newsletter.Models.Dtos;

namespace Newsletter.Services;

public interface IEmailVerificationService
{
    Task<ServiceResult<SendVerificationCodeResponse>> SendCodeAsync(string? email, CancellationToken cancellationToken);

    Task<ServiceResult<ConfirmVerificationCodeResponse>> ConfirmCodeAsync(string? email, string? code, CancellationToken cancellationToken);
}
