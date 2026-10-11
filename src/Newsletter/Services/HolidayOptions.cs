namespace Newsletter.Services;

/// <summary>
/// 특일정보 API 설정. BaseUrl은 appsettings.json, ServiceKey는 User Secrets에 둔다 (security.md)
/// </summary>
public class HolidayOptions
{
    public const string SectionName = "Holiday";

    /// <summary>
    /// 공휴일 정보 조회(getRestDeInfo) 주소
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 공공데이터포털 일반 인증키 (Decoding 키)
    /// </summary>
    public string ServiceKey { get; set; } = string.Empty;
}
