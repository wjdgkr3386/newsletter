using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Newsletter.Services;

/// <summary>
/// 공공데이터포털 한국천문연구원 특일정보 API의 공휴일 정보 조회(getRestDeInfo) (D-044)
/// </summary>
public class SpecialDayApiClient : IHolidayApiClient
{
    // 한 해 공휴일은 20개 안팎이라 100개면 한 번에 모두 받는다
    private const int RowsPerRequest = 100;
    private const string SuccessResultCode = "00";

    private readonly HttpClient _httpClient;
    private readonly HolidayOptions _holidayOptions;
    private readonly ILogger<SpecialDayApiClient> _logger;

    public SpecialDayApiClient(HttpClient httpClient, IOptions<HolidayOptions> holidayOptions, ILogger<SpecialDayApiClient> logger)
    {
        _httpClient = httpClient;
        _holidayOptions = holidayOptions.Value;
        _logger = logger;
    }

    public async Task<List<HolidayInfo>?> GetHolidaysAsync(int year, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_holidayOptions.ServiceKey))
        {
            _logger.LogError("특일정보 API 키(Holiday:ServiceKey)가 설정되지 않았습니다.");
            return null;
        }

        string requestUrl = _holidayOptions.BaseUrl
            + "?solYear=" + year.ToString(CultureInfo.InvariantCulture)
            + "&numOfRows=" + RowsPerRequest.ToString(CultureInfo.InvariantCulture)
            + "&_type=json"
            + "&ServiceKey=" + Uri.EscapeDataString(_holidayOptions.ServiceKey);

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("특일정보 API 응답 오류: {Year}년, HTTP {StatusCode}", year, (int)response.StatusCode);
                return null;
            }

            List<HolidayInfo>? holidays = ParseResponse(body, out string? errorReason);
            if (holidays is null)
            {
                _logger.LogError("특일정보 API 응답을 해석하지 못했습니다: {Year}년, {Reason}", year, errorReason);
            }

            return holidays;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            // 요청 주소에 API 키가 있으므로 주소는 남기지 않는다 (security.md)
            _logger.LogError("특일정보 API 호출 실패: {Year}년, {ExceptionType}", year, exception.GetType().Name);
            return null;
        }
    }

    /// <summary>
    /// 응답 JSON을 해석한다. item이 1건이면 객체, 여러 건이면 배열, 0건이면 items가 빈 문자열로 온다.
    /// 공휴일(isHoliday = "Y")만 돌려준다. 해석할 수 없으면 null과 이유를 돌려준다
    /// </summary>
    public static List<HolidayInfo>? ParseResponse(string body, out string? errorReason)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            JsonElement responseElement = document.RootElement.GetProperty("response");

            string? resultCode = responseElement.GetProperty("header").GetProperty("resultCode").GetString();
            if (resultCode != SuccessResultCode)
            {
                errorReason = "resultCode " + resultCode;
                return null;
            }

            List<HolidayInfo> holidays = [];
            JsonElement itemsElement = responseElement.GetProperty("body").GetProperty("items");
            if (itemsElement.ValueKind != JsonValueKind.Object || !itemsElement.TryGetProperty("item", out JsonElement itemElement))
            {
                errorReason = null;
                return holidays;
            }

            IEnumerable<JsonElement> items = itemElement.ValueKind == JsonValueKind.Array ? itemElement.EnumerateArray() : [itemElement];
            foreach (JsonElement item in items)
            {
                if (item.GetProperty("isHoliday").GetString() != "Y")
                {
                    continue;
                }

                // locdate는 20260101 같은 숫자로 온다
                string locdate = item.GetProperty("locdate").ToString();
                DateOnly date = DateOnly.ParseExact(locdate, "yyyyMMdd", CultureInfo.InvariantCulture);
                holidays.Add(new HolidayInfo(date, item.GetProperty("dateName").GetString() ?? string.Empty));
            }

            errorReason = null;
            return holidays;
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            // 키가 잘못되면 JSON 대신 XML 오류가 오는 경우가 있다
            errorReason = "응답 형식이 올바르지 않음 (" + exception.GetType().Name + ")";
            return null;
        }
    }
}
