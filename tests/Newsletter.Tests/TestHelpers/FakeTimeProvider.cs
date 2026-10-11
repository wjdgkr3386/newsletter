namespace Newsletter.Tests.TestHelpers;

/// <summary>
/// 현재 시각을 테스트에서 정하는 TimeProvider
/// </summary>
public class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTime utcNow)
    {
        _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void SetUtcNow(DateTime utcNow)
    {
        _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);
    }
}
