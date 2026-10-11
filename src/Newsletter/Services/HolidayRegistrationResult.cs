namespace Newsletter.Services;

/// <summary>
/// 공휴일 등록 결과
/// </summary>
public enum HolidayRegistrationResult
{
    /// <summary>
    /// 그해 공휴일이 이미 있어 API를 부르지 않았다
    /// </summary>
    AlreadyRegistered,

    /// <summary>
    /// API로 받아 새로 등록했다
    /// </summary>
    Registered,

    /// <summary>
    /// API 호출 실패 또는 0건. 다음 확인 때 다시 시도한다 (D-080)
    /// </summary>
    Failed,
}
