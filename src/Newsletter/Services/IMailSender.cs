namespace Newsletter.Services;

public interface IMailSender
{
    /// <summary>
    /// 메일을 보낸다. 성공하면 true, 실패하면 false (실패 원인은 로그에만 남긴다)
    /// </summary>
    Task<bool> SendAsync(string toAddress, string subject, string textBody, CancellationToken cancellationToken);
}
