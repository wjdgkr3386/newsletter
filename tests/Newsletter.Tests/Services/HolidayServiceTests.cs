using Microsoft.Extensions.Logging.Abstractions;
using Newsletter.Data;
using Newsletter.Models.Entities;
using Newsletter.Services;
using Newsletter.Tests.TestHelpers;

namespace Newsletter.Tests.Services;

/// <summary>
/// 공휴일 등록과 발송 제외일 판단 (D-031, D-044, D-080) 테스트
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
        _dbContext.Holidays.Add(new Holiday { HolidayDate = date, Name = name, CreatedAt = Now, UpdatedAt = Now });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    // ---------- 등록 ----------
    [Fact]
    public async Task Ensure_NoDataForYear_RegistersFromApi()
    {
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 1, 1), "1월1일"), new HolidayInfo(new DateOnly(2026, 3, 2), "대체공휴일")];

        HolidayRegistrationResult result = await _service.EnsureYearRegisteredAsync(2026, CancellationToken.None);

        Assert.Equal(HolidayRegistrationResult.Registered, result);
        Assert.Equal(2, _dbContext.Holidays.Count());
        Assert.Equal(Now, _dbContext.Holidays.First().CreatedAt);
    }

    [Fact]
    public async Task Ensure_DataExists_DoesNotCallApi()
    {
        await AddHolidayAsync(new DateOnly(2026, 12, 25), "기독탄신일"); // 경계: 그해 마지막 공휴일만 있어도 등록된 것으로 본다

        HolidayRegistrationResult result = await _service.EnsureYearRegisteredAsync(2026, CancellationToken.None);

        Assert.Equal(HolidayRegistrationResult.AlreadyRegistered, result);
        Assert.Equal(0, _apiClient.CallCount);
    }

    [Fact]
    public async Task Ensure_OnlyPreviousYearExists_CallsApi()
    {
        await AddHolidayAsync(new DateOnly(2025, 12, 25), "기독탄신일"); // 경계: 전년 12월 31일 이전 데이터는 올해로 보지 않는다
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 1, 1), "1월1일")];

        HolidayRegistrationResult result = await _service.EnsureYearRegisteredAsync(2026, CancellationToken.None);

        Assert.Equal(HolidayRegistrationResult.Registered, result);
        Assert.Equal(1, _apiClient.CallCount);
    }

    [Fact]
    public async Task Ensure_ApiFails_ReturnsFailed()
    {
        _apiClient.Holidays = null;

        HolidayRegistrationResult result = await _service.EnsureYearRegisteredAsync(2026, CancellationToken.None);

        Assert.Equal(HolidayRegistrationResult.Failed, result);
        Assert.Empty(_dbContext.Holidays);
    }

    [Fact]
    public async Task Ensure_ApiReturnsNothing_ReturnsFailed()
    {
        _apiClient.Holidays = []; // 경계: 0건은 아직 자료 없음으로 보고 다시 시도한다 (D-080)

        HolidayRegistrationResult result = await _service.EnsureYearRegisteredAsync(2026, CancellationToken.None);

        Assert.Equal(HolidayRegistrationResult.Failed, result);
    }

    [Fact]
    public async Task Ensure_SameDateTwice_SavesOneRowWithFirstName()
    {
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 5, 5), "어린이날"), new HolidayInfo(new DateOnly(2026, 5, 5), "다른 이름")];

        await _service.EnsureYearRegisteredAsync(2026, CancellationToken.None);

        Holiday saved = Assert.Single(_dbContext.Holidays);
        Assert.Equal("어린이날", saved.Name);
    }

    [Fact]
    public async Task Ensure_OtherYearDateInResponse_IsIgnored()
    {
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 1, 1), "1월1일"), new HolidayInfo(new DateOnly(2027, 1, 1), "1월1일")];

        await _service.EnsureYearRegisteredAsync(2026, CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 1, 1), Assert.Single(_dbContext.Holidays).HolidayDate);
    }

    [Fact]
    public async Task Ensure_LongName_IsCutTo50()
    {
        _apiClient.Holidays = [new HolidayInfo(new DateOnly(2026, 1, 1), new string('가', 51))]; // 경계: 51자

        await _service.EnsureYearRegisteredAsync(2026, CancellationToken.None);

        Assert.Equal(50, Assert.Single(_dbContext.Holidays).Name.Length);
    }

    // ---------- 발송 제외일 ----------
    [Theory]
    [InlineData(2026, 10, 9, false)] // 금요일 (한글날은 아래에서 따로)
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

        Assert.True(await _service.IsNonWorkingDayAsync(new DateOnly(2026, 10, 9), CancellationToken.None));
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
