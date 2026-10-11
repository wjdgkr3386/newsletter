using Newsletter.Data;
using Newsletter.Models.Entities;
using Newsletter.Services;
using Newsletter.Tests.TestHelpers;

namespace Newsletter.Tests.Services;

/// <summary>
/// 로그인 아이디·비밀번호 확인 (api.md 6.1 POST /api/auth/login) 테스트
/// </summary>
public class AuthServiceTests
{
    private const string LoginId = "newsuser01";
    private const string Password = "abcd1234!";

    private readonly NewsletterDbContext _dbContext = TestDbContextFactory.Create();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_dbContext);
    }

    private async Task AddUserAsync(string role)
    {
        _dbContext.Users.Add(new User
        {
            LoginId = LoginId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
            Email = "user@gmail.com",
            Name = "홍길동",
            Role = role,
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(UserRole.User)]
    [InlineData(UserRole.Admin)]
    public async Task Validate_CorrectCredentials_ReturnsUser(string role)
    {
        await AddUserAsync(role);

        ServiceResult<User> result = await _service.ValidateCredentialsAsync(LoginId, Password, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(LoginId, result.Data!.LoginId);
        Assert.Equal(role, result.Data.Role);
    }

    [Fact]
    public async Task Validate_WrongPassword_ReturnsLoginFailed()
    {
        await AddUserAsync(UserRole.User);

        ServiceResult<User> result = await _service.ValidateCredentialsAsync(LoginId, "abcd1234@", CancellationToken.None);

        Assert.Equal(ErrorCodes.LoginFailed, result.Error!.Code);
        Assert.Equal(401, result.Error.StatusCode);
    }

    [Fact]
    public async Task Validate_UnknownLoginId_ReturnsSameMessageAsWrongPassword()
    {
        await AddUserAsync(UserRole.User);

        ServiceResult<User> unknownIdResult = await _service.ValidateCredentialsAsync("unknown01", Password, CancellationToken.None);
        ServiceResult<User> wrongPasswordResult = await _service.ValidateCredentialsAsync(LoginId, "wrong1234!", CancellationToken.None);

        // 아이디와 비밀번호 중 무엇이 틀렸는지 알리지 않는다 (security.md 2)
        Assert.Equal(ErrorCodes.LoginFailed, unknownIdResult.Error!.Code);
        Assert.Equal(wrongPasswordResult.Error!.Message, unknownIdResult.Error.Message);
    }

    [Fact]
    public async Task Validate_LoginIdCaseDifferent_ReturnsLoginFailed()
    {
        await AddUserAsync(UserRole.User);

        // 메모리 DB는 대소문자를 구분한다. 실제 SQL Server는 대소문자를 구분하지 않지만 가입 시 소문자만 허용한다 (D-052)
        ServiceResult<User> result = await _service.ValidateCredentialsAsync("NEWSUSER01", Password, CancellationToken.None);

        Assert.Equal(ErrorCodes.LoginFailed, result.Error!.Code);
    }

    [Theory]
    [InlineData(null, Password)]
    [InlineData("", Password)]
    [InlineData(LoginId, null)]
    [InlineData(LoginId, "")]
    public async Task Validate_EmptyInput_ReturnsInvalidInput(string? loginId, string? password)
    {
        await AddUserAsync(UserRole.User);

        ServiceResult<User> result = await _service.ValidateCredentialsAsync(loginId, password, CancellationToken.None);

        Assert.Equal(ErrorCodes.InvalidInput, result.Error!.Code);
        Assert.Equal(400, result.Error.StatusCode);
    }

    [Fact]
    public async Task Validate_LoginIdLength21_ReturnsLoginFailed()
    {
        await AddUserAsync(UserRole.User);

        ServiceResult<User> result = await _service.ValidateCredentialsAsync(new string('a', 21), Password, CancellationToken.None); // 경계: 21자

        Assert.Equal(ErrorCodes.LoginFailed, result.Error!.Code);
    }

    [Fact]
    public async Task Validate_PasswordLength21_ReturnsLoginFailed()
    {
        await AddUserAsync(UserRole.User);

        ServiceResult<User> result = await _service.ValidateCredentialsAsync(LoginId, Password + "123456789012", CancellationToken.None); // 경계: 21자

        Assert.Equal(ErrorCodes.LoginFailed, result.Error!.Code);
    }
}
