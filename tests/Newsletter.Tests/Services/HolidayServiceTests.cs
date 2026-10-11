using Microsoft.Extensions.Logging.Abstractions;
using Newsletter.Data;
using Newsletter.Models.Entities;
using Newsletter.Services;
using Newsletter.Tests.TestHelpers;

namespace Newsletter.Tests.Services;

/// <summary>
/// 공휴일 동기화와 발송 제외일 판단 (D-031, D-044, D-080, D-082) 테스트
/// </summary>
public class HolidayServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly NewsletterDbContext _dbContext = TestDbContextFactory.Create();
    private readonly FakeHolidayApiClient _apiClient = new();
    private readonly HolidayService _service;

    public HolidayServiceTests()
    {
        _service = new HolidayService(_dbContext, _apiClient, new FakeTimeProvider(Now), NullLogger<HolidayService>.Instance);
    }

    private async Task AddHolidayAsync(DateOnly date, string name)
    {
        _dbContext.Holidays.Add(new Holiday { HolidayDate = date, Name = name, CreatedAt = Now.AddDays(-1), UpdatedAt = Now.AddDays(-1) });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    // ---------- 동기화 ----------
    [Fact]
    public async Task Sync_EmptyDb_AddsAll()
    {
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 1, 1), "1월1일"), new HolidayInfo(new DateOnly(2026, 3, 2), "대체공휴일(삼일절)")];

        HolidaySyncResult result = await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.Equal(new HolidaySyncResult(true, 2), result);
        Assert.Equal(2, _dbContext.Holidays.Count());
        Assert.Equal(Now, _dbContext.Holidays.First().CreatedAt);
    }

    [Fact]
    public async Task Sync_NewTemporaryHoliday_AddsOnlyMissingDate()
    {
        await AddHolidayAsync(new DateOnly(2026, 1, 1), "1월1일");
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 1, 1), "1월1일"), new HolidayInfo(new DateOnly(2026, 6, 3), "전국동시지방선거")];

        HolidaySyncResult result = await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.Equal(new HolidaySyncResult(true, 1), result);
        Assert.Equal(2, _dbContext.Holidays.Count());
    }

    [Fact]
    public async Task Sync_NothingNew_SucceedsWithZeroAdded()
    {
        await AddHolidayAsync(new DateOnly(2026, 1, 1), "1월1일");
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 1, 1), "1월1일")];

        HolidaySyncResult result = await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.Equal(new HolidaySyncResult(true, 0), result);
        Assert.Single(_dbContext.Holidays);
    }

    [Fact]
    public async Task Sync_ExistingDateWithOtherName_IsNotChanged()
    {
        await AddHolidayAsync(new DateOnly(2026, 5, 5), "어린이날");
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 5, 5), "바뀐 이름")];

        await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.Equal("어린이날", Assert.Single(_dbContext.Holidays).Name);
    }

    [Fact]
    public async Task Sync_HolidayMissingFromApi_IsNotDeleted()
    {
        await AddHolidayAsync(new DateOnly(2026, 6, 3), "전국동시지방선거");
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 1, 1), "1월1일")];

        await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.Equal(2, _dbContext.Holidays.Count()); // 지우지 않는다 (D-082)
    }

    [Fact]
    public async Task Sync_ApiFails_ReturnsFailure()
    {
        _apiClient.Holidays = null;

        HolidaySyncResult result = await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(_dbContext.Holidays);
    }

    [Fact]
    public async Task Sync_ApiReturnsNothing_ReturnsFailure()
    {
        _apiClient.Holidays = []; // 경계: 0건은 아직 자료 없음으로 보고 다시 시도한다 (D-080)

        HolidaySyncResult result = await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Sync_OnlyOtherYearInResponse_ReturnsFailure()
    {
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2027, 1, 1), "1월1일")];

        HolidaySyncResult result = await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(_dbContext.Holidays);
    }

    [Fact]
    public async Task Sync_OtherYearDateInResponse_IsIgnored()
    {
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 12, 31), "임시"), new HolidayInfo(new DateOnly(2027, 1, 1), "1월1일")]; // 경계: 연말과 다음 해 첫날

        await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 12, 31), Assert.Single(_dbContext.Holidays).HolidayDate);
    }

    [Fact]
    public async Task Sync_SameDateTwice_SavesOneRowWithFirstName()
    {
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 5, 5), "어린이날"), new HolidayInfo(new DateOnly(2026, 5, 5), "다른 이름")];

        await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.Equal("어린이날", Assert.Single(_dbContext.Holidays).Name);
    }

    [Theory]
    [InlineData(50, 50)] // 경계: 50자
    [InlineData(51, 50)] // 경계: 51자는 50자로 자른다
    public async Task Sync_LongName_IsCutTo50(int nameLength, int expectedLength)
    {
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 1, 1), new string('가', nameLength))];

        await _service.SyncYearAsync(2026, CancellationToken.None);

        Assert.Equal(expectedLength, Assert.Single(_dbContext.Holidays).Name.Length);
    }

    // ---------- 발송 제외일 ----------
    [Theory]
    [InlineData(2026, 10, 8, false)] // 목요일
    [InlineData(2026, 10, 10, true)] // 경계: 토요일
    [InlineData(2026, 10, 11, true)] // 경계: 일요일
    [InlineData(2026, 10, 12, false)] // 경계: 월요일
    public async Task IsNonWorkingDay_ByWeekday(int year, int month, int day, bool expected)
    {
        Assert.Equal(expected, await _service.IsNonWorkingDayAsync(new DateOnly(year, month, day), CancellationToken.None));
    }

    [Fact]
    public async Task IsNonWorkingDay_WeekdayHoliday_ReturnsTrue()
    {
        await AddHolidayAsync(new DateOnly(2026, 10, 9), "한글날");

        Assert.True(await _service.IsNonWorkingDayAsync(new DateOnly(2026, 10, 9), CancellationToken.None)); // 금요일 공휴일
        Assert.False(await _service.IsNonWorkingDayAsync(new DateOnly(2026, 10, 8), CancellationToken.None)); // 경계: 하루 전 평일
    }

    [Fact]
    public async Task IsNonWorkingDay_YearBoundary()
    {
        await AddHolidayAsync(new DateOnly(2027, 1, 1), "1월1일");

        Assert.False(await _service.IsNonWorkingDayAsync(new DateOnly(2026, 12, 31), CancellationToken.None)); // 목요일
        Assert.True(await _service.IsNonWorkingDayAsync(new DateOnly(2027, 1, 1), CancellationToken.None));
    }
}
