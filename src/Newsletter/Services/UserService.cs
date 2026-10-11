using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;

namespace Newsletter.Services;

/// <summary>
/// 회원가입 (D-019, D-032, D-052, D-061)
/// </summary>
public class UserService : IUserService
{
    private readonly NewsletterDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public UserService(NewsletterDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<ServiceResult<RegisterUserResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        string? name = request.Name?.Trim();
        string? inputError = InputValidator.ValidateLoginId(request.LoginId)
            ?? InputValidator.ValidatePassword(request.Password)
            ?? InputValidator.ValidateEmail(request.Email)
            ?? InputValidator.ValidateName(name);
        if (inputError is not null)
        {
            return ServiceResult<RegisterUserResponse>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput, inputError);
        }

        string loginId = request.LoginId!;
        string email = request.Email!;

        ServiceError? duplicateError = await FindDuplicateErrorAsync(loginId, email, cancellationToken);
        if (duplicateError is not null)
        {
            return ServiceResult<RegisterUserResponse>.Failure(duplicateError);
        }

        // 그 이메일의 가장 최근 인증 행이 인증 완료여야 한다. 인증 후 가입까지 시간 제한은 없다 (D-061)
        bool isEmailVerified = await _dbContext.EmailVerifications
            .Where(verification => verification.Email == email)
            .OrderByDescending(verification => verification.Id)
            .Select(verification => verification.IsVerified)
            .FirstOrDefaultAsync(cancellationToken);
        if (!isEmailVerified)
        {
            return ServiceResult<RegisterUserResponse>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.EmailNotVerified, "이메일 인증을 완료해 주세요.");
        }

        DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        User user = new()
        {
            LoginId = loginId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password!),
            Email = email,
            Name = name!,
            Role = UserRole.User,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc,
        };
        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // 중복 검사 직후 다른 요청이 같은 아이디나 이메일로 먼저 가입한 경우 (유니크 인덱스 위반)
            _dbContext.Entry(user).State = EntityState.Detached;
            ServiceError? raceDuplicateError = await FindDuplicateErrorAsync(loginId, email, cancellationToken);
            if (raceDuplicateError is not null)
            {
                return ServiceResult<RegisterUserResponse>.Failure(raceDuplicateError);
            }

            throw;
        }

        return ServiceResult<RegisterUserResponse>.Success(new RegisterUserResponse(user.Id));
    }

    private async Task<ServiceError?> FindDuplicateErrorAsync(string loginId, string email, CancellationToken cancellationToken)
    {
        if (await _dbContext.Users.AnyAsync(user => user.LoginId == loginId, cancellationToken))
        {
            return new ServiceError(StatusCodes.Status409Conflict, ErrorCodes.LoginIdDuplicated, "이미 사용 중인 아이디입니다.");
        }

        if (await _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            return new ServiceError(StatusCodes.Status409Conflict, ErrorCodes.EmailDuplicated, "이미 사용 중인 이메일입니다.");
        }

        return null;
    }
}
