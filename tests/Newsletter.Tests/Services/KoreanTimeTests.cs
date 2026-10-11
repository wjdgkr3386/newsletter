using Newsletter.Services;
using Newsletter.Tests.TestHelpers;

namespace Newsletter.Tests.Services;

/// <summary>
/// 한국 시간(KST = UTC+9) 날짜 계산 경계값 테스트
/// </summary>
public class KoreanTimeTests
{
    [Theory]
    [InlineData(2026, 12, 31, 14, 59, 2026, 12, 31)] // 경계: KST 12월 31일 23:59
    [InlineData(2026, 12, 31, 15, 0, 2027, 1, 1)] // 경계: KST 1월 1일 00:00
    [InlineData(2026, 10, 10, 15, 0, 2026, 10, 11)]
    public void GetToday_ConvertsUtcToKst(int year, int month, int day, int hour, int minute, int expectedYear, int expectedMonth, int expectedDay)
    {
        FakeTimeProvider timeProvider = new(new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc));

        Assert.Equal(new DateOnly(expectedYear, expectedMonth, expectedDay), KoreanTime.GetToday(timeProvider));
    }
}
