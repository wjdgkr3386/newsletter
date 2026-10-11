namespace Newsletter.Services;

/// <summary>
/// 특일정보 API에서 받은 공휴일 하나
/// </summary>
public record HolidayInfo(DateOnly Date, string Name);
