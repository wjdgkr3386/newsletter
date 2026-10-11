using System.Text.RegularExpressions;

namespace Newsletter.Services;

/// <summary>
/// 회원가입 입력값 검증 (D-052). 통과하면 null, 실패하면 오류 메시지를 반환한다
/// </summary>
public static partial class InputValidator
{
    public const int LoginIdMinLength = 6;
    public const int LoginIdMaxLength = 20;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 20;
    public const int NameMinLength = 2;
    public const int NameMaxLength = 30;
    public const int EmailMaxLength = 50;
    public const int VerificationCodeLength = 6;

    // 공백을 뺀 ASCII 특수문자 32개 (D-052)
    public const string AllowedSpecialCharacters = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

    [GeneratedRegex("^[a-z0-9]+$")]
    private static partial Regex LoginIdPattern();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();

    [GeneratedRegex("^[0-9]{6}$")]
    private static partial Regex VerificationCodePattern();

    public static string? ValidateLoginId(string? loginId)
    {
        if (string.IsNullOrEmpty(loginId)
            || loginId.Length < LoginIdMinLength
            || loginId.Length > LoginIdMaxLength
            || !LoginIdPattern().IsMatch(loginId))
        {
            return "아이디는 영문 소문자와 숫자 6~20자로 입력해 주세요.";
        }

        return null;
    }

    public static string? ValidatePassword(string? password)
    {
        const string message = "비밀번호는 8~20자이며 영문, 숫자, 특수문자를 각각 1자 이상 포함해야 합니다.";

        if (string.IsNullOrEmpty(password) || password.Length < PasswordMinLength || password.Length > PasswordMaxLength)
        {
            return message;
        }

        bool hasLetter = false;
        bool hasDigit = false;
        bool hasSpecial = false;

        foreach (char character in password)
        {
            if (char.IsAsciiLetter(character))
            {
                hasLetter = true;
            }
            else if (char.IsAsciiDigit(character))
            {
                hasDigit = true;
            }
            else if (AllowedSpecialCharacters.Contains(character))
            {
                hasSpecial = true;
            }
            else
            {
                // 영문, 숫자, 허용된 특수문자 외의 문자(공백, 한글 등)는 쓸 수 없다
                return message;
            }
        }

        return hasLetter && hasDigit && hasSpecial ? null : message;
    }

    public static string? ValidateEmail(string? email)
    {
        if (string.IsNullOrEmpty(email) || email.Length > EmailMaxLength || !EmailPattern().IsMatch(email))
        {
            return "이메일 형식이 올바르지 않습니다. (최대 50자)";
        }

        return null;
    }

    // 앞뒤 공백은 호출하는 쪽에서 잘라서 넘긴다
    public static string? ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < NameMinLength || name.Length > NameMaxLength)
        {
            return "이름은 2~30자로 입력해 주세요.";
        }

        return null;
    }

    public static string? ValidateVerificationCode(string? code)
    {
        if (string.IsNullOrEmpty(code) || !VerificationCodePattern().IsMatch(code))
        {
            return "인증 코드는 6자리 숫자로 입력해 주세요.";
        }

        return null;
    }
}
