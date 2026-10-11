namespace Newsletter.Services;

/// <summary>
/// 이메일 인증 코드 규칙 (D-032: 6자리 숫자, 유효 시간 10분 / D-068: 5번 틀리면 무효)
/// </summary>
public static class VerificationRules
{
    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    public const int MaxFailCount = 5;

    /// <summary>
    /// 틀린 횟수가 최대치에 이르면 true (그 코드는 더 이상 쓸 수 없다)
    /// </summary>
    public static bool IsLocked(int failCount)
    {
        return failCount >= MaxFailCount;
    }

    public static DateTime GetExpiresAt(DateTime sentAtUtc)
    {
        // datetime2(0) 컬럼에 맞춰 초 단위로 자른다
        DateTime truncated = new(sentAtUtc.Ticks - (sentAtUtc.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        return truncated.Add(CodeLifetime);
    }

    /// <summary>
    /// 만료 시각이 지났으면 true. 만료 시각과 같은 순간까지는 유효하다
    /// </summary>
    public static bool IsExpired(DateTime expiresAtUtc, DateTime nowUtc)
    {
        return nowUtc > expiresAtUtc;
    }
}
