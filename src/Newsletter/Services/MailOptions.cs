namespace Newsletter.Services;

/// <summary>
/// 메일 발송 설정. SmtpHost, SmtpPort, SenderName은 appsettings.json, SenderAddress와 AppPassword는 User Secrets에 둔다
/// </summary>
public class MailOptions
{
    public const string SectionName = "Mail";

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string SenderAddress { get; set; } = string.Empty;

    public string AppPassword { get; set; } = string.Empty;
}
