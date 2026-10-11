using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Models.Entities;

namespace Newsletter.Services;

/// <summary>
/// 공휴일 등록과 발송 제외일 판단 (D-031, D-037, D-044, D-080)
/// </summary>
public class HolidayService : IHolidayService
{
    private const int NameMaxLength = 50;

    private readonly NewsletterDbContext _dbContext;
    private readonly IHolidayApiClient _holidayApiClient;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<HolidayService> _logger;

    public HolidayService(NewsletterDbContext dbContext, IHolidayApiClient holidayApiClient, TimeProvider timeProvider, ILogger<HolidayService> logger)
    {
        _dbContext = dbContext;
        _holidayApiClient = holidayApiClient;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<HolidayRegistrationResult> EnsureYearRegisteredAsync(int year, CancellationToken cancellationToken)
    {
        DateOnly firstDay = new(year, 1, 1);
        DateOnly lastDay = new(year, 12, 31);
        bool isRegistered = await _dbContext.Holidays.AnyAsync(holiday => holiday.HolidayDate >= firstDay && holiday.HolidayDate <= lastDay, cancellationToken);
        if (isRegistered)
        {
            return HolidayRegistrationResult.AlreadyRegistered;
        }

        List<HolidayInfo>? holidays = await _holidayApiClient.GetHolidaysAsync(year, cancellationToken);
        if (holidays is null || holidays.Count == 0)
        {
            // 0건은 아직 그해 자료가 없는 것으로 보고 다시 시도한다 (D-080)
            _logger.LogWarning("{Year}년 공휴일을 등록하지 못했습니다.", year);
            return HolidayRegistrationResult.Failed;
        }

        DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        List<Holiday> newHolidays = holidays
            .Where(holiday => holiday.Date.Year == year)
            .GroupBy(holiday => holiday.Date)
            .Select(group => new Holiday
            {
                HolidayDate = group.Key,
                // 같은 날짜에 공휴일이 둘 이상이면 먼저 받은 이름으로 한 행만 저장한다 (database.md 4.7)
                Name = TrimName(group.First().Name),
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc,
            })
            .ToList();
        if (newHolidays.Count == 0)
        {
            _logger.LogWarning("{Year}년 공휴일 응답에 그해 날짜가 없습니다.", year);
            return HolidayRegistrationResult.Failed;
        }

        _dbContext.Holidays.AddRange(newHolidays);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Year}년 공휴일 {Count}일을 등록했습니다.", year, newHolidays.Count);
        return HolidayRegistrationResult.Registered;
    }

    public async Task<bool> IsNonWorkingDayAsync(DateOnly date, CancellationToken cancellationToken)
    {
        if (IsWeekend(date))
        {
            return true;
        }

        return await _dbContext.Holidays.AnyAsync(holiday => holiday.HolidayDate == date, cancellationToken);
    }

    public static bool IsWeekend(DateOnly date)
    {
        return date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }

    private static string TrimName(string name)
    {
        return name.Length <= NameMaxLength ? name : name[..NameMaxLength];
    }
}
