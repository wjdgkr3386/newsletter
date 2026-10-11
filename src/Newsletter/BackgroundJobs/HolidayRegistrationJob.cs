using Newsletter.Services;

namespace Newsletter.BackgroundJobs;

/// <summary>
/// 앱 시작 직후와 1시간마다 올해(KST) 공휴일이 DB에 있는지 확인하고, 없으면 특일정보 API로 받아 등록한다.
/// 1월 1일 등록과 실패 시 재시도를 이 확인 하나로 처리한다 (D-037, D-044, D-080)
/// </summary>
public class HolidayRegistrationJob : BackgroundService
{
    public static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<HolidayRegistrationJob> _logger;

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
            await CheckCurrentYearAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckCurrentYearAsync(CancellationToken stoppingToken)
    {
        int year = KoreanTime.GetToday(_timeProvider).Year;
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IHolidayService holidayService = scope.ServiceProvider.GetRequiredService<IHolidayService>();
            HolidayRegistrationResult result = await holidayService.EnsureYearRegisteredAsync(year, stoppingToken);
            if (result == HolidayRegistrationResult.Failed)
            {
                _logger.LogWarning("{Year}년 공휴일 등록에 실패했습니다. {Minutes}분 뒤 다시 시도합니다.", year, (int)CheckInterval.TotalMinutes);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // DB 연결 오류 등이 나도 앱은 멈추지 않고 다음 확인 때 다시 시도한다
            _logger.LogError(exception, "{Year}년 공휴일 확인 중 오류가 발생했습니다.", year);
        }
    }
}
