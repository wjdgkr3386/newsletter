using Newsletter.Data;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;
using Newsletter.Services;
using Newsletter.Tests.TestHelpers;

namespace Newsletter.Tests.Services;

/// <summary>
/// 회원가입 (api.md 6.1 POST /api/users) 테스트
/// </summary>
public class UserServiceTests
{
    private const string LoginId = "newsuser01";
    private const string Password = "abcd1234!";
    private const string Email = "user@gmail.com";
    private const string Name = "홍길동";
    private static readonly DateTime Now = new(2026, 10, 7, 10, 0, 0, DateTimeKind.Utc);

    private readonly NewsletterDbContext _dbContext = TestDbContextFactory.Create();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(_dbContext, new FakeTimeProvider(Now));
    }

    private async Task AddVerificationAsync(string email, bool isVerified)
    {
        _dbContext.EmailVerifications.Add(new EmailVerification
        {
            Email = email,
            Code = "123456",
            ExpiresAt = Now.AddMinutes(10),
            IsVerified = isVerified,
            CreatedAt = Now,
            UpdatedAt = Now,
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task AddUserAsync(string loginId, string email)
    {
        _dbContext.Users.Add(new User { LoginId = loginId, PasswordHash = "hash", Email = email, Name = "기존회원" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Register_VerifiedEmail_SavesUserWithHashAndUserRole()
    {
        await AddVerificationAsync(Email, true);

        ServiceResult<RegisterUserResponse> result = await _service.RegisterAsync(new RegisterUserRequest(LoginId, Password, Email, "  " + Name + " "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        User saved = Assert.Single(_dbContext.Users);
        Assert.Equal(result.Data!.Id, saved.Id);
        Assert.Equal(LoginId, saved.LoginId);
        Assert.Equal(Name, saved.Name); // 앞뒤 공백 제거
        Assert.Equal(UserRole.User, saved.Role);
        Assert.Equal(60, saved.PasswordHash.Length);
        Assert.NotEqual(Password, saved.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(Password, saved.PasswordHash));
    }

    [Fact]
    public async Task Register_NoVerification_ReturnsEmailNotVerified()
    {
        ServiceResult<RegisterUserResponse> result = await _service.RegisterAsync(new RegisterUserRequest(LoginId, Password, Email, Name), CancellationToken.None);

        Assert.Equal(ErrorCodes.EmailNotVerified, result.Error!.Code);
        Assert.Empty(_dbContext.Users);
    }

    [Fact]
    public async Task Register_LatestVerificationNotVerified_ReturnsEmailNotVerified()
    {
        await AddVerificationAsync(Email, true);
        await AddVerificationAsync(Email, false); // 재발송 후 아직 확인하지 않음

        ServiceResult<RegisterUserResponse> result = await _service.RegisterAsync(new RegisterUserRequest(LoginId, Password, Email, Name), CancellationToken.None);

        Assert.Equal(ErrorCodes.EmailNotVerified, result.Error!.Code);
    }

    [Fact]
    public async Task Register_VerifiedOtherEmailOnly_ReturnsEmailNotVerified()
    {
        await AddVerificationAsync("other@gmail.com", true);

        ServiceResult<RegisterUserResponse> result = await _service.RegisterAsync(new RegisterUserRequest(LoginId, Password, Email, Name), CancellationToken.None);

        Assert.Equal(ErrorCodes.EmailNotVerified, result.Error!.Code);
    }

    [Fact]
    public async Task Register_DuplicateLoginId_ReturnsLoginIdDuplicated()
    {
        await AddUserAsync(LoginId, "other@gmail.com");
        await AddVerificationAsync(Email, true);

        ServiceResult<RegisterUserResponse> result = await _service.RegisterAsync(new RegisterUserRequest(LoginId, Password, Email, Name), CancellationToken.None);

        Assert.Equal(ErrorCodes.LoginIdDuplicated, result.Error!.Code);
        Assert.Equal(409, result.Error.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsEmailDuplicated()
    {
        await AddUserAsync("otheruser01", Email);
        await AddVerificationAsync(Email, true);

        ServiceResult<RegisterUserResponse> result = await _service.RegisterAsync(new RegisterUserRequest(LoginId, Password, Email, Name), CancellationToken.None);

        Assert.Equal(ErrorCodes.EmailDuplicated, result.Error!.Code);
    }

    [Theory]
    [InlineData("abc12", Password, Email, Name)] // 아이디 5자
    [InlineData(LoginId, "abcd1234", Email, Name)] // 비밀번호 특수문자 없음
    [InlineData(LoginId, Password, "not-email", Name)] // 이메일 형식
    [InlineData(LoginId, Password, Email, "홍")] // 이름 1자
    [InlineData(null, Password, Email, Name)] // 아이디 없음
    public async Task Register_InvalidInput_ReturnsInvalidInput(string? loginId, string? password, string? email, string? name)
    {
        await AddVerificationAsync(Email, true);

        ServiceResult<RegisterUserResponse> result = await _service.RegisterAsync(new RegisterUserRequest(loginId, password, email, name), CancellationToken.None);

        Assert.Equal(ErrorCodes.InvalidInput, result.Error!.Code);
        Assert.Equal(400, result.Error.StatusCode);
        Assert.Empty(_dbContext.Users);
    }
}
