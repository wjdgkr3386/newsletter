using System.Text.RegularExpressions;
using Newsletter.Data;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;
using Newsletter.Services;
using Newsletter.Tests.TestHelpers;

namespace Newsletter.Tests.Services;

/// <summary>
/// 인증 코드 발송·확인 (api.md 6.1) 테스트
/// </summary>
public class EmailVerificationServiceTests
{
    private const string Email = "user@gmail.com";
    private static readonly DateTime SentAt = new(2026, 10, 7, 10, 0, 0, DateTimeKind.Utc);

    private readonly NewsletterDbContext _dbContext = TestDbContextFactory.Create();
    private readonly FakeMailSender _mailSender = new();
    private readonly FakeTimeProvider _timeProvider = new(SentAt);
    private readonly EmailVerificationService _service;

    public EmailVerificationServiceTests()
    {
        _service = new EmailVerificationService(_dbContext, _mailSender, _timeProvider);
    }

    private string GetSentCode(int index = 0)
    {
        return Regex.Match(_mailSender.SentBodies[index], "[0-9]{6}").Value;
    }

    // ---------- 발송 ----------
    [Fact]
    public async Task SendCode_ValidEmail_SendsMailAndSavesRow()
    {
        ServiceResult<SendVerificationCodeResponse> result = await _service.SendCodeAsync(Email, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SentAt.AddMinutes(10), result.Data!.ExpiresAt);
        Assert.Equal([Email], _mailSender.SentAddresses);
        EmailVerification saved = Assert.Single(_dbContext.EmailVerifications);
        Assert.Equal(GetSentCode(), saved.Code);
        Assert.False(saved.IsVerified);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-email")]
    public async Task SendCode_InvalidEmail_ReturnsInvalidInput(string? email)
    {
        ServiceResult<SendVerificationCodeResponse> result = await _service.SendCodeAsync(email, CancellationToken.None);

        Assert.Equal(ErrorCodes.InvalidInput, result.Error!.Code);
        Assert.Empty(_mailSender.SentAddresses);
    }

    [Fact]
    public async Task SendCode_EmailAlreadyUsed_ReturnsEmailDuplicated()
    {
        _dbContext.Users.Add(new User { LoginId = "user0001", PasswordHash = "hash", Email = Email, Name = "홍길동" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        ServiceResult<SendVerificationCodeResponse> result = await _service.SendCodeAsync(Email, CancellationToken.None);

        Assert.Equal(ErrorCodes.EmailDuplicated, result.Error!.Code);
        Assert.Equal(409, result.Error.StatusCode);
        Assert.Empty(_mailSender.SentAddresses);
    }

    [Fact]
    public async Task SendCode_MailFails_ReturnsMailSendFailedAndSavesNothing()
    {
        _mailSender.IsSendSuccess = false;

        ServiceResult<SendVerificationCodeResponse> result = await _service.SendCodeAsync(Email, CancellationToken.None);

        Assert.Equal(ErrorCodes.MailSendFailed, result.Error!.Code);
        Assert.Empty(_dbContext.EmailVerifications);
    }

    // ---------- 확인 ----------
    [Fact]
    public async Task ConfirmCode_CorrectCode_MarksVerified()
    {
        await _service.SendCodeAsync(Email, CancellationToken.None);

        ServiceResult<ConfirmVerificationCodeResponse> result = await _service.ConfirmCodeAsync(Email, GetSentCode(), CancellationToken.None);

        Assert.True(result.Data!.IsVerified);
        Assert.True(Assert.Single(_dbContext.EmailVerifications).IsVerified);
    }

    [Fact]
    public async Task ConfirmCode_WrongCode_ReturnsCodeInvalid()
    {
        await _service.SendCodeAsync(Email, CancellationToken.None);
        string wrongCode = GetSentCode() == "000000" ? "111111" : "000000";

        ServiceResult<ConfirmVerificationCodeResponse> result = await _service.ConfirmCodeAsync(Email, wrongCode, CancellationToken.None);

        Assert.Equal(ErrorCodes.VerificationCodeInvalid, result.Error!.Code);
        EmailVerification saved = Assert.Single(_dbContext.EmailVerifications);
        Assert.False(saved.IsVerified);
        Assert.Equal(1, saved.FailCount);
        Assert.Contains("남은 횟수 4번", result.Error.Message);
    }

    // ---------- 5번 틀리면 무효 (D-068) ----------
    private string GetWrongCode()
    {
        return GetSentCode() == "000000" ? "111111" : "000000";
    }

    private async Task<ServiceResult<ConfirmVerificationCodeResponse>> ConfirmWrongAsync(int count)
    {
        ServiceResult<ConfirmVerificationCodeResponse> lastResult = null!;
        for (int index = 0; index < count; index++)
        {
            lastResult = await _service.ConfirmCodeAsync(Email, GetWrongCode(), CancellationToken.None);
        }

        return lastResult;
    }

    [Fact]
    public async Task ConfirmCode_FourWrongThenCorrect_Succeeds()
    {
        await _service.SendCodeAsync(Email, CancellationToken.None);
        ServiceResult<ConfirmVerificationCodeResponse> fourthResult = await ConfirmWrongAsync(4); // 경계: 4번

        ServiceResult<ConfirmVerificationCodeResponse> result = await _service.ConfirmCodeAsync(Email, GetSentCode(), CancellationToken.None);

        Assert.Equal(ErrorCodes.VerificationCodeInvalid, fourthResult.Error!.Code);
        Assert.Contains("남은 횟수 1번", fourthResult.Error.Message);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ConfirmCode_FifthWrong_ReturnsLocked()
    {
        await _service.SendCodeAsync(Email, CancellationToken.None);

        ServiceResult<ConfirmVerificationCodeResponse> fifthResult = await ConfirmWrongAsync(5); // 경계: 5번

        Assert.Equal(ErrorCodes.VerificationCodeLocked, fifthResult.Error!.Code);
        Assert.Equal(5, Assert.Single(_dbContext.EmailVerifications).FailCount);
    }

    [Fact]
    public async Task ConfirmCode_CorrectAfterLocked_ReturnsLocked()
    {
        await _service.SendCodeAsync(Email, CancellationToken.None);
        await ConfirmWrongAsync(5);

        ServiceResult<ConfirmVerificationCodeResponse> result = await _service.ConfirmCodeAsync(Email, GetSentCode(), CancellationToken.None);

        Assert.Equal(ErrorCodes.VerificationCodeLocked, result.Error!.Code);
        Assert.Equal(5, Assert.Single(_dbContext.EmailVerifications).FailCount); // 무효가 된 뒤에는 더 세지 않는다
    }

    [Fact]
    public async Task ConfirmCode_ResendAfterLocked_NewCodeSucceeds()
    {
        await _service.SendCodeAsync(Email, CancellationToken.None);
        await ConfirmWrongAsync(5);
        await _service.SendCodeAsync(Email, CancellationToken.None);

        ServiceResult<ConfirmVerificationCodeResponse> result = await _service.ConfirmCodeAsync(Email, GetSentCode(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ConfirmCode_NoCodeSent_ReturnsNotFound()
    {
        ServiceResult<ConfirmVerificationCodeResponse> result = await _service.ConfirmCodeAsync(Email, "123456", CancellationToken.None);

        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
    }

    [Fact]
    public async Task ConfirmCode_AtExpiresAt_Succeeds()
    {
        await _service.SendCodeAsync(Email, CancellationToken.None);
        _timeProvider.SetUtcNow(SentAt.AddMinutes(10)); // 경계: 정확히 10분

        ServiceResult<ConfirmVerificationCodeResponse> result = await _service.ConfirmCodeAsync(Email, GetSentCode(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ConfirmCode_OneSecondAfterExpiresAt_ReturnsExpired()
    {
        await _service.SendCodeAsync(Email, CancellationToken.None);
        _timeProvider.SetUtcNow(SentAt.AddMinutes(10).AddSeconds(1)); // 경계: 10분 1초

        ServiceResult<ConfirmVerificationCodeResponse> result = await _service.ConfirmCodeAsync(Email, GetSentCode(), CancellationToken.None);

        Assert.Equal(ErrorCodes.VerificationCodeExpired, result.Error!.Code);
    }

    [Fact]
    public async Task ConfirmCode_PreviousCodeAfterResend_ReturnsCodeInvalid()
    {
        await _service.SendCodeAsync(Email, CancellationToken.None);
        await _service.SendCodeAsync(Email, CancellationToken.None);
        string previousCode = GetSentCode(0);
        string latestCode = GetSentCode(1);
        if (previousCode == latestCode)
        {
            return; // 두 코드가 우연히 같으면 이 경우를 확인할 수 없다 (확률 100만분의 1)
        }

        ServiceResult<ConfirmVerificationCodeResponse> previousResult = await _service.ConfirmCodeAsync(Email, previousCode, CancellationToken.None);
        ServiceResult<ConfirmVerificationCodeResponse> latestResult = await _service.ConfirmCodeAsync(Email, latestCode, CancellationToken.None);

        Assert.Equal(ErrorCodes.VerificationCodeInvalid, previousResult.Error!.Code);
        Assert.True(latestResult.IsSuccess);
    }
}
