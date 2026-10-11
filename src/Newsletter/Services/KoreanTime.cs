namespace Newsletter.Services;

/// <summary>
/// 한국 시간(KST) 계산. 자동 실행과 공휴일 판단은 KST 기준이다 (D-036)
/// </summary>
public static class KoreanTime
{
    private static readonly TimeZoneInfo KoreaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");

    public static DateTime ToKoreanTime(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), KoreaTimeZone);
    }

    public static DateOnly GetToday(TimeProvider timeProvider)
    {
        return DateOnly.FromDateTime(ToKoreanTime(timeProvider.GetUtcNow().UtcDateTime));
    }
}
