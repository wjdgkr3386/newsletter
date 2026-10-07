using Newsletter.Models.Dtos;

namespace Newsletter.Services;

public interface IUserService
{
    Task<ServiceResult<RegisterUserResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
}
