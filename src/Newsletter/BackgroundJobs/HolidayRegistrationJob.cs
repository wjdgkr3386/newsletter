using Newsletter.Services;

namespace Newsletter.BackgroundJobs;

/// <summary>
/// 1시간마다 확인해서, 오늘(KST) 아직 동기화하지 않았으면 올해 공휴일을 특일정보 API로 받아 없는 날짜만 추가한다.
/// 앱 시작 직후 첫 동기화, 1월 1일 등록, 매일 동기화(임시공휴일 반영), 실패 시 1시간 뒤 재시도를 이 확인 하나로 처리한다
/// (D-037, D-044, D-080, D-082)
/// </summary>
public class HolidayRegistrationJob : BackgroundService
{
    public static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<HolidayRegistrationJob> _logger;

    // 마지막으로 동기화에 성공한 날(KST). 앱을 다시 시작하면 비어 있으므로 시작 직후 한 번 동기화한다
    private DateOnly? _lastSyncedDate;

    public HolidayRegistrationJob(IServiceScopeFactory scopeFactory, TimeProvider timeProvider, ILogger<HolidayRegistrationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(CheckInterval, _timeProvider);
        do
        {
            await SyncIfNeededAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>
    /// 오늘 이미 동기화했으면 false (다시 받지 않는다)
    /// </summary>
    public static bool NeedsSync(DateOnly? lastSyncedDate, DateOnly today)
    {
        return lastSyncedDate != today;
    }

    private async Task SyncIfNeededAsync(CancellationToken stoppingToken)
    {
        DateOnly today = KoreanTime.GetToday(_timeProvider);
        if (!NeedsSync(_lastSyncedDate, today))
        {
            return;
        }

        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IHolidayService holidayService = scope.ServiceProvider.GetRequiredService<IHolidayService>();
            HolidaySyncResult result = await holidayService.SyncYearAsync(today.Year, stoppingToken);
            if (result.IsSuccess)
            {
                _lastSyncedDate = today;
                return;
            }

            _logger.LogWarning("{Year}년 공휴일 동기화에 실패했습니다. {Minutes}분 뒤 다시 시도합니다.", today.Year, (int)CheckInterval.TotalMinutes);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // DB 연결 오류 등이 나도 앱은 멈추지 않고 다음 확인 때 다시 시도한다
            _logger.LogError(exception, "{Year}년 공휴일 동기화 중 오류가 발생했습니다.", today.Year);
        }
    }
}
