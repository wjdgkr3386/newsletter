using Newsletter.Services;

namespace Newsletter.Tests.Services;

/// <summary>
/// 특일정보 API 응답 해석 테스트 (item 0건·1건·여러 건, 오류 응답)
/// </summary>
public class SpecialDayApiClientTests
{
    private const string Header = "\"header\":{\"resultCode\":\"00\",\"resultMsg\":\"NORMAL SERVICE.\"}";

    private static string Wrap(string items, int totalCount)
    {
        return "{\"response\":{" + Header + ",\"body\":{\"items\":" + items + ",\"numOfRows\":100,\"pageNo\":1,\"totalCount\":" + totalCount + "}}}";
    }

    [Fact]
    public void Parse_ManyItems_ReturnsHolidays()
    {
        string body = Wrap(
            "{\"item\":[" +
            "{\"dateKind\":\"01\",\"dateName\":\"1월1일\",\"isHoliday\":\"Y\",\"locdate\":20260101,\"seq\":1}," +
            "{\"dateKind\":\"01\",\"dateName\":\"대체공휴일\",\"isHoliday\":\"Y\",\"locdate\":20260302,\"seq\":1}," +
            "{\"dateKind\":\"01\",\"dateName\":\"기념일\",\"isHoliday\":\"N\",\"locdate\":20260405,\"seq\":1}" +
            "]}",
            3);

        List<HolidayInfo>? holidays = SpecialDayApiClient.ParseResponse(body, out string? errorReason);

        Assert.Null(errorReason);
        Assert.Equal(
            [new HolidayInfo(new DateOnly(2026, 1, 1), "1월1일"), new HolidayInfo(new DateOnly(2026, 3, 2), "대체공휴일")],
            holidays);
    }

    [Fact]
    public void Parse_OneItemAsObject_ReturnsOneHoliday()
    {
        string body = Wrap("{\"item\":{\"dateKind\":\"01\",\"dateName\":\"추석\",\"isHoliday\":\"Y\",\"locdate\":20261005,\"seq\":1}}", 1);

        List<HolidayInfo>? holidays = SpecialDayApiClient.ParseResponse(body, out _);

        Assert.Equal([new HolidayInfo(new DateOnly(2026, 10, 5), "추석")], holidays);
    }

    [Fact]
    public void Parse_NoItems_ReturnsEmpty()
    {
        string body = Wrap("\"\"", 0);

        List<HolidayInfo>? holidays = SpecialDayApiClient.ParseResponse(body, out string? errorReason);

        Assert.NotNull(holidays);
        Assert.Empty(holidays);
        Assert.Null(errorReason);
    }

    [Fact]
    public void Parse_ErrorResultCode_ReturnsNull()
    {
        string body = "{\"response\":{\"header\":{\"resultCode\":\"30\",\"resultMsg\":\"SERVICE_KEY_IS_NOT_REGISTERED_ERROR\"}}}";

        List<HolidayInfo>? holidays = SpecialDayApiClient.ParseResponse(body, out string? errorReason);

        Assert.Null(holidays);
        Assert.Equal("resultCode 30", errorReason);
    }

    [Theory]
    [InlineData("<OpenAPI_ServiceResponse><cmmMsgHeader><errMsg>SERVICE ERROR</errMsg></cmmMsgHeader></OpenAPI_ServiceResponse>")] // XML 오류 응답
    [InlineData("")]
    [InlineData("{\"response\":{}}")] // header 없음
    public void Parse_InvalidBody_ReturnsNull(string body)
    {
        List<HolidayInfo>? holidays = SpecialDayApiClient.ParseResponse(body, out string? errorReason);

        Assert.Null(holidays);
        Assert.NotNull(errorReason);
    }
}
