namespace Newsletter.Services;

public interface IHolidayService
{
    /// <summary>
    /// 그해 공휴일이 DB에 없으면 API로 받아 등록한다
    /// </summary>
    Task<HolidayRegistrationResult> EnsureYearRegisteredAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// 자동 생성·발송을 하지 않는 날(주말, 공휴일, 대체공휴일)이면 true (D-031)
    /// </summary>
    Task<bool> IsNonWorkingDayAsync(DateOnly date, CancellationToken cancellationToken);
}
