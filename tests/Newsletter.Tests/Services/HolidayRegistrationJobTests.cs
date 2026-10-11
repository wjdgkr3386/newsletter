using Newsletter.BackgroundJobs;

namespace Newsletter.Tests.Services;

/// <summary>
/// 공휴일 동기화 판단 (하루 한 번, D-082) 테스트
/// </summary>
public class HolidayRegistrationJobTests
{
    private static readonly DateOnly Today = new(2026, 10, 11);

    [Fact]
    public void NeedsSync_NeverSynced_ReturnsTrue() // 앱 시작 직후
    {
        Assert.True(HolidayRegistrationJob.NeedsSync(null, Today));
    }

    [Fact]
    public void NeedsSync_SyncedToday_ReturnsFalse()
    {
        Assert.False(HolidayRegistrationJob.NeedsSync(Today, Today));
    }

    [Fact]
    public void NeedsSync_SyncedYesterday_ReturnsTrue() // 경계: 날짜가 바뀐 뒤 첫 확인
    {
        Assert.True(HolidayRegistrationJob.NeedsSync(Today.AddDays(-1), Today));
    }

    [Fact]
    public void NeedsSync_NewYear_ReturnsTrue() // 경계: 12월 31일 → 1월 1일
    {
        Assert.True(HolidayRegistrationJob.NeedsSync(new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 1)));
    }
}
