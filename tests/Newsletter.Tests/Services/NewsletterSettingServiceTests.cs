using Newsletter.Data;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;
using Newsletter.Services;
using Newsletter.Tests.TestHelpers;

namespace Newsletter.Tests.Services;

/// <summary>
/// 뉴스레터 설정 (api.md 6.3, D-043) 동등 분할, 경계값 테스트
/// </summary>
public class NewsletterSettingServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 11, 10, 0, 0, DateTimeKind.Utc);

    private readonly NewsletterDbContext _dbContext = TestDbContextFactory.Create();
    private readonly NewsletterSettingService _service;

    public NewsletterSettingServiceTests()
    {
        _service = new NewsletterSettingService(_dbContext, new FakeTimeProvider(Now));
        _dbContext.NewsletterSettings.Add(new NewsletterSetting
        {
            GenerateTime = new TimeOnly(7, 50),
            SendTime = new TimeOnly(8, 0),
            CreatedAt = Now.AddDays(-1),
            UpdatedAt = Now.AddDays(-1),
        });
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Get_ReturnsHourMinuteFormat()
    {
        NewsletterSettingResponse setting = await _service.GetAsync(CancellationToken.None);

        Assert.Equal("07:50", setting.GenerateTime);
        Assert.Equal("08:00", setting.SendTime);
    }

    [Fact]
    public async Task Save_ValidTimes_UpdatesSingleRow()
    {
        ServiceResult<NewsletterSettingResponse> result = await _service.SaveAsync("06:30", "07:00", CancellationToken.None);

        Assert.Equal(new NewsletterSettingResponse("06:30", "07:00"), result.Data);
        NewsletterSetting saved = Assert.Single(_dbContext.NewsletterSettings);
        Assert.Equal(new TimeOnly(6, 30), saved.GenerateTime);
        Assert.Equal(new TimeOnly(7, 0), saved.SendTime);
        Assert.Equal(Now, saved.UpdatedAt);
    }

    [Theory]
    [InlineData("07:59", "08:00")] // 경계: 1분 차이
    [InlineData("00:00", "23:59")] // 경계: 하루의 처음과 끝
    [InlineData("23:58", "23:59")] // 경계: 가장 늦은 설정
    [InlineData("00:00", "00:01")] // 경계: 가장 이른 설정
    public async Task Save_GenerateBeforeSend_Succeeds(string generateTime, string sendTime)
    {
        ServiceResult<NewsletterSettingResponse> result = await _service.SaveAsync(generateTime, sendTime, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("08:00", "08:00")] // 경계: 같음
    [InlineData("08:01", "08:00")] // 경계: 1분 늦음
    [InlineData("23:59", "00:00")] // 하루를 넘기는 설정은 허용하지 않음
    public async Task Save_GenerateNotBeforeSend_ReturnsInvalidSettingTime(string generateTime, string sendTime)
    {
        ServiceResult<NewsletterSettingResponse> result = await _service.SaveAsync(generateTime, sendTime, CancellationToken.None);

        Assert.Equal(ErrorCodes.InvalidSettingTime, result.Error!.Code);
        Assert.Equal(400, result.Error.StatusCode);
        Assert.Equal(new TimeOnly(7, 50), Assert.Single(_dbContext.NewsletterSettings).GenerateTime); // 바뀌지 않음
    }

    [Theory]
    [InlineData(null, "08:00")]
    [InlineData("", "08:00")]
    [InlineData("7:50", "08:00")] // 시 한 자리
    [InlineData("07:50:00", "08:00")] // 초 포함
    [InlineData("24:00", "08:00")] // 경계: 범위 밖 시
    [InlineData("07:60", "08:00")] // 경계: 범위 밖 분
    [InlineData("07:50", "abc")] // 형식 아님
    [InlineData("07:50", null)]
    public async Task Save_InvalidFormat_ReturnsInvalidInput(string? generateTime, string? sendTime)
    {
        ServiceResult<NewsletterSettingResponse> result = await _service.SaveAsync(generateTime, sendTime, CancellationToken.None);

        Assert.Equal(ErrorCodes.InvalidInput, result.Error!.Code);
    }

    [Theory]
    [InlineData("00:00", true)] // 경계: 가장 작은 값
    [InlineData("23:59", true)] // 경계: 가장 큰 값
    [InlineData("12:30", true)]
    [InlineData("24:00", false)]
    [InlineData("-1:00", false)]
    [InlineData("1230", false)]
    public void TryParseTime_ByFormat(string value, bool expected)
    {
        Assert.Equal(expected, NewsletterSettingService.TryParseTime(value, out _));
    }
}
