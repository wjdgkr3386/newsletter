using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Newsletter.Services;

/// <summary>
/// Gmail SMTP로 메일을 보낸다 (D-006, MailKit)
/// </summary>
public class GmailMailSender : IMailSender
{
    private readonly MailOptions _mailOptions;
    private readonly ILogger<GmailMailSender> _logger;

    public GmailMailSender(IOptions<MailOptions> mailOptions, ILogger<GmailMailSender> logger)
    {
        _mailOptions = mailOptions.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string toAddress, string subject, string textBody, CancellationToken cancellationToken)
    {
        MimeMessage message = new();
        message.From.Add(new MailboxAddress(_mailOptions.SenderName, _mailOptions.SenderAddress));
        message.To.Add(MailboxAddress.Parse(toAddress));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = textBody };

        try
        {
            using SmtpClient smtpClient = new();
            await smtpClient.ConnectAsync(_mailOptions.SmtpHost, _mailOptions.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await smtpClient.AuthenticateAsync(_mailOptions.SenderAddress, _mailOptions.AppPassword, cancellationToken);
            await smtpClient.SendAsync(message, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // 비밀 정보는 남기지 않고 예외 종류와 메시지만 로그에 남긴다
            _logger.LogError("메일 발송 실패: {ExceptionType} {ExceptionMessage}", exception.GetType().Name, exception.Message);
            return false;
        }
    }
}
