namespace Newsletter.Services;

public interface IHolidayApiClient
{
    /// <summary>
    /// 그해 공휴일(대체공휴일 포함)을 받는다. 실패하면 null (원인은 로그에만 남긴다)
    /// </summary>
    Task<List<HolidayInfo>?> GetHolidaysAsync(int year, CancellationToken cancellationToken);
}
