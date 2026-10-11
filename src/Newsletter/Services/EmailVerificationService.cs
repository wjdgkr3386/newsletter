using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;

namespace Newsletter.Services;

/// <summary>
/// 이메일 인증 코드 발송과 확인 (D-020, D-032)
/// </summary>
public class EmailVerificationService : IEmailVerificationService
{
    private const string MailSubject = "[뉴스레터] 이메일 인증 코드";

    private readonly NewsletterDbContext _dbContext;
    private readonly IMailSender _mailSender;
    private readonly TimeProvider _timeProvider;

    public EmailVerificationService(NewsletterDbContext dbContext, IMailSender mailSender, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _mailSender = mailSender;
        _timeProvider = timeProvider;
    }

    public async Task<ServiceResult<SendVerificationCodeResponse>> SendCodeAsync(string? email, CancellationToken cancellationToken)
    {
        string? emailError = InputValidator.ValidateEmail(email);
        if (emailError is not null)
        {
            return ServiceResult<SendVerificationCodeResponse>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput, emailError);
        }

        string validEmail = email!;
        bool isEmailUsed = await _dbContext.Users.AnyAsync(user => user.Email == validEmail, cancellationToken);
        if (isEmailUsed)
        {
            return ServiceResult<SendVerificationCodeResponse>.Failure(StatusCodes.Status409Conflict, ErrorCodes.EmailDuplicated, "이미 사용 중인 이메일입니다.");
        }

        DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        string code = CreateCode();
        EmailVerification verification = new()
        {
            Email = validEmail,
            Code = code,
            ExpiresAt = VerificationRules.GetExpiresAt(nowUtc),
            IsVerified = false,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc,
        };

        // 메일 발송에 성공한 코드만 저장한다. 다시 보내면 새 행을 만들고, 확인에는 가장 최근 행만 쓴다
        string mailBody = $"뉴스레터 회원가입 인증 코드는 {code} 입니다.\n10분 안에 회원가입 화면에 입력해 주세요.";
        bool isSent = await _mailSender.SendAsync(validEmail, MailSubject, mailBody, cancellationToken);
        if (!isSent)
        {
            return ServiceResult<SendVerificationCodeResponse>.Failure(StatusCodes.Status500InternalServerError, ErrorCodes.MailSendFailed, "인증 코드 메일을 보내지 못했습니다. 잠시 후 다시 시도해 주세요.");
        }

        _dbContext.EmailVerifications.Add(verification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<SendVerificationCodeResponse>.Success(new SendVerificationCodeResponse(verification.ExpiresAt));
    }

    public async Task<ServiceResult<ConfirmVerificationCodeResponse>> ConfirmCodeAsync(string? email, string? code, CancellationToken cancellationToken)
    {
        string? inputError = InputValidator.ValidateEmail(email) ?? InputValidator.ValidateVerificationCode(code);
        if (inputError is not null)
        {
            return ServiceResult<ConfirmVerificationCodeResponse>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput, inputError);
        }

        EmailVerification? latestVerification = await _dbContext.EmailVerifications
            .Where(verification => verification.Email == email)
            .OrderByDescending(verification => verification.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (latestVerification is null)
        {
            return ServiceResult<ConfirmVerificationCodeResponse>.Failure(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "인증 코드를 먼저 발송해 주세요.");
        }

        if (VerificationRules.IsLocked(latestVerification.FailCount))
        {
            return LockedFailure();
        }

        DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        if (VerificationRules.IsExpired(latestVerification.ExpiresAt, nowUtc))
        {
            return ServiceResult<ConfirmVerificationCodeResponse>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.VerificationCodeExpired, "인증 코드가 만료되었습니다. 재발송해 주세요.");
        }

        bool isCodeMatched = CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(latestVerification.Code),
            Encoding.ASCII.GetBytes(code!));
        if (!isCodeMatched)
        {
            // 틀린 횟수를 저장하고, 5번째로 틀리면 그 코드를 무효로 한다 (D-068)
            latestVerification.FailCount++;
            latestVerification.UpdatedAt = nowUtc;
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (VerificationRules.IsLocked(latestVerification.FailCount))
            {
                return LockedFailure();
            }

            int remainingCount = VerificationRules.MaxFailCount - latestVerification.FailCount;
            return ServiceResult<ConfirmVerificationCodeResponse>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.VerificationCodeInvalid, $"인증 코드가 맞지 않습니다. (남은 횟수 {remainingCount}번)");
        }

        if (!latestVerification.IsVerified)
        {
            latestVerification.IsVerified = true;
            latestVerification.UpdatedAt = nowUtc;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return ServiceResult<ConfirmVerificationCodeResponse>.Success(new ConfirmVerificationCodeResponse(true));
    }

    private static ServiceResult<ConfirmVerificationCodeResponse> LockedFailure()
    {
        return ServiceResult<ConfirmVerificationCodeResponse>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.VerificationCodeLocked, "인증 코드를 5번 틀려 더 이상 쓸 수 없습니다. 재발송해 주세요.");
    }

    private static string CreateCode()
    {
        // 암호학적 난수로 000000~999999 중 하나를 만든다
        return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    }
}
