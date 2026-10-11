using Newsletter.Services;

namespace Newsletter.Tests.TestHelpers;

/// <summary>
/// 실제로 메일을 보내지 않고 보낸 내용을 기록하는 IMailSender
/// </summary>
public class FakeMailSender : IMailSender
{
    public bool IsSendSuccess { get; set; } = true;

    public List<string> SentAddresses { get; } = [];

    public List<string> SentBodies { get; } = [];

    public Task<bool> SendAsync(string toAddress, string subject, string textBody, CancellationToken cancellationToken)
    {
        if (IsSendSuccess)
        {
            SentAddresses.Add(toAddress);
            SentBodies.Add(textBody);
        }

        return Task.FromResult(IsSendSuccess);
    }
}
