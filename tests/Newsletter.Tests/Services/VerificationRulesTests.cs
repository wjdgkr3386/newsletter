using Newsletter.Services;

namespace Newsletter.Tests.Services;

/// <summary>
/// 인증 코드 유효 시간 10분 (D-032) 경계값 테스트
/// </summary>
public class VerificationRulesTests
{
    private static readonly DateTime ExpiresAt = new(2026, 10, 7, 10, 10, 0, DateTimeKind.Utc);

    [Fact]
    public void GetExpiresAt_AddsTenMinutesAndTruncatesToSecond()
    {
        DateTime sentAt = new DateTime(2026, 10, 7, 10, 0, 0, DateTimeKind.Utc).AddMilliseconds(999);
        Assert.Equal(ExpiresAt, VerificationRules.GetExpiresAt(sentAt));
    }

    [Fact]
    public void IsExpired_OneSecondBefore_ReturnsFalse()
    {
        Assert.False(VerificationRules.IsExpired(ExpiresAt, ExpiresAt.AddSeconds(-1)));
    }

    [Fact]
    public void IsExpired_AtExpiresAt_ReturnsFalse()
    {
        Assert.False(VerificationRules.IsExpired(ExpiresAt, ExpiresAt));
    }

    [Fact]
    public void IsExpired_OneSecondAfter_ReturnsTrue()
    {
        Assert.True(VerificationRules.IsExpired(ExpiresAt, ExpiresAt.AddSeconds(1)));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(4, false)] // 경계: 4번
    [InlineData(5, true)] // 경계: 5번
    [InlineData(6, true)]
    public void IsLocked_ByFailCount(int failCount, bool expected)
    {
        Assert.Equal(expected, VerificationRules.IsLocked(failCount));
    }

    [Fact]
    public void IsExpired_JustSent_ReturnsFalse()
    {
        Assert.False(VerificationRules.IsExpired(ExpiresAt, ExpiresAt.AddMinutes(-10)));
    }
}
