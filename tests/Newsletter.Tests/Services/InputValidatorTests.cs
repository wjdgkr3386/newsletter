using Newsletter.Services;

namespace Newsletter.Tests.Services;

/// <summary>
/// 회원가입 입력값 검증 (D-052) 동등 분할, 경계값 테스트
/// </summary>
public class InputValidatorTests
{
    // ---------- 아이디: 영문 소문자+숫자 6~20자 ----------
    [Theory]
    [InlineData("abc123")] // 경계: 6자
    [InlineData("abcd1234")] // 대표값
    [InlineData("abcdefghij1234567890")] // 경계: 20자
    [InlineData("abcdef")] // 소문자만
    [InlineData("123456")] // 숫자만
    public void ValidateLoginId_Valid_ReturnsNull(string loginId)
    {
        Assert.Null(InputValidator.ValidateLoginId(loginId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc12")] // 경계: 5자
    [InlineData("abcdefghij12345678901")] // 경계: 21자
    [InlineData("Abc1234")] // 대문자
    [InlineData("abc_1234")] // 특수문자
    [InlineData("abc 1234")] // 공백
    [InlineData("한글아이디12")] // 한글
    public void ValidateLoginId_Invalid_ReturnsMessage(string? loginId)
    {
        Assert.NotNull(InputValidator.ValidateLoginId(loginId));
    }

    // ---------- 비밀번호: 8~20자, 영문·숫자·특수문자 각 1자 이상 ----------
    [Theory]
    [InlineData("abcd12!@")] // 경계: 8자
    [InlineData("Abcd1234!")] // 대표값
    [InlineData("abcdefgh1234567890!@")] // 경계: 20자
    [InlineData("ABCD123~")] // 대문자만 영문
    public void ValidatePassword_Valid_ReturnsNull(string password)
    {
        Assert.Null(InputValidator.ValidatePassword(password));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc12!@")] // 경계: 7자
    [InlineData("abcdefgh1234567890!@#")] // 경계: 21자
    [InlineData("12345678!")] // 영문 없음
    [InlineData("abcdefgh!")] // 숫자 없음
    [InlineData("abcd1234")] // 특수문자 없음
    [InlineData("abcd 1234!")] // 공백
    [InlineData("abcd1234!가")] // 한글
    public void ValidatePassword_Invalid_ReturnsMessage(string? password)
    {
        Assert.NotNull(InputValidator.ValidatePassword(password));
    }

    [Fact]
    public void ValidatePassword_EveryAllowedSpecialCharacter_ReturnsNull()
    {
        Assert.Equal(32, InputValidator.AllowedSpecialCharacters.Length);
        foreach (char specialCharacter in InputValidator.AllowedSpecialCharacters)
        {
            Assert.Null(InputValidator.ValidatePassword("abcd1234" + specialCharacter));
        }
    }

    // ---------- 이메일: 기본 형식, 최대 50자 ----------
    [Theory]
    [InlineData("user@gmail.com")] // 대표값
    [InlineData("a@b.c")] // 짧은 형식
    public void ValidateEmail_Valid_ReturnsNull(string email)
    {
        Assert.Null(InputValidator.ValidateEmail(email));
    }

    [Fact]
    public void ValidateEmail_Length50_ReturnsNull()
    {
        string email = new string('a', 40) + "@gmail.com"; // 경계: 50자
        Assert.Equal(50, email.Length);
        Assert.Null(InputValidator.ValidateEmail(email));
    }

    [Fact]
    public void ValidateEmail_Length51_ReturnsMessage()
    {
        string email = new string('a', 41) + "@gmail.com"; // 경계: 51자
        Assert.Equal(51, email.Length);
        Assert.NotNull(InputValidator.ValidateEmail(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("usergmail.com")] // @ 없음
    [InlineData("user@gmail")] // 도메인에 점 없음
    [InlineData("user name@gmail.com")] // 공백
    [InlineData("@gmail.com")] // 로컬 부분 없음
    public void ValidateEmail_Invalid_ReturnsMessage(string? email)
    {
        Assert.NotNull(InputValidator.ValidateEmail(email));
    }

    // ---------- 이름: 2~30자 ----------
    [Theory]
    [InlineData("홍길")] // 경계: 2자
    [InlineData("홍길동")] // 대표값
    [InlineData("Gildong Hong")] // 영문과 공백
    public void ValidateName_Valid_ReturnsNull(string name)
    {
        Assert.Null(InputValidator.ValidateName(name));
    }

    [Fact]
    public void ValidateName_Length30_ReturnsNull()
    {
        Assert.Null(InputValidator.ValidateName(new string('가', 30))); // 경계: 30자
    }

    [Fact]
    public void ValidateName_Length31_ReturnsMessage()
    {
        Assert.NotNull(InputValidator.ValidateName(new string('가', 31))); // 경계: 31자
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("홍")] // 경계: 1자
    [InlineData("   ")] // 공백만
    public void ValidateName_Invalid_ReturnsMessage(string? name)
    {
        Assert.NotNull(InputValidator.ValidateName(name));
    }

    // ---------- 인증 코드: 6자리 숫자 ----------
    [Theory]
    [InlineData("000000")]
    [InlineData("123456")]
    public void ValidateVerificationCode_Valid_ReturnsNull(string code)
    {
        Assert.Null(InputValidator.ValidateVerificationCode(code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")] // 경계: 5자리
    [InlineData("1234567")] // 경계: 7자리
    [InlineData("12a456")] // 문자 포함
    public void ValidateVerificationCode_Invalid_ReturnsMessage(string? code)
    {
        Assert.NotNull(InputValidator.ValidateVerificationCode(code));
    }
}
