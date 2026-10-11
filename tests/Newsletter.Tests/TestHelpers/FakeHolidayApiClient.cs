using Newsletter.Services;

namespace Newsletter.Tests.TestHelpers;

/// <summary>
/// 실제 특일정보 API를 부르지 않고 정해 둔 공휴일을 돌려주는 IHolidayApiClient
/// </summary>
public class FakeHolidayApiClient : IHolidayApiClient
{
    /// <summary>
    /// null이면 API 실패로 본다
    /// </summary>
    public List<HolidayInfo>? Holidays { get; set; } = [];

    public int CallCount { get; private set; }

    public Task<List<HolidayInfo>?> GetHolidaysAsync(int year, CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(Holidays);
    }
}
