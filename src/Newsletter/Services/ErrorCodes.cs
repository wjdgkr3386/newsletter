namespace Newsletter.Services;

/// <summary>
/// api.md "4. 오류 코드" 목록
/// </summary>
public static class ErrorCodes
{
    public const string InvalidInput = "INVALID_INPUT";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string InternalError = "INTERNAL_ERROR";
    public const string VerificationCodeInvalid = "VERIFICATION_CODE_INVALID";
    public const string VerificationCodeExpired = "VERIFICATION_CODE_EXPIRED";
    public const string VerificationCodeLocked = "VERIFICATION_CODE_LOCKED";
    public const string EmailNotVerified = "EMAIL_NOT_VERIFIED";
    public const string LoginIdDuplicated = "LOGIN_ID_DUPLICATED";
    public const string EmailDuplicated = "EMAIL_DUPLICATED";
    public const string MailSendFailed = "MAIL_SEND_FAILED";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string AlreadyRecipient = "ALREADY_RECIPIENT";
}
