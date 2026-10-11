using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;

namespace Newsletter.Services;

/// <summary>
/// 뉴스레터 자동 생성·발송 시간 설정 (D-036, D-043). newsletter_settings는 1행만 쓴다
/// </summary>
public class NewsletterSettingService : INewsletterSettingService
{
    public const string TimeFormat = "HH:mm";

    private readonly NewsletterDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public NewsletterSettingService(NewsletterDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<NewsletterSettingResponse> GetAsync(CancellationToken cancellationToken)
    {
        NewsletterSetting setting = await FindSettingAsync(cancellationToken);
        return ToResponse(setting);
    }

    public async Task<ServiceResult<NewsletterSettingResponse>> SaveAsync(string? generateTime, string? sendTime, CancellationToken cancellationToken)
    {
        if (!TryParseTime(generateTime, out TimeOnly parsedGenerateTime) || !TryParseTime(sendTime, out TimeOnly parsedSendTime))
        {
            return ServiceResult<NewsletterSettingResponse>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput, "시간은 HH:mm 형식(00:00~23:59)으로 입력해 주세요.");
        }

        // 생성 시간은 발송 시간보다 앞서야 한다 (D-043, DB 제약 ck_newsletter_settings_time과 같은 규칙)
        if (parsedGenerateTime >= parsedSendTime)
        {
            return ServiceResult<NewsletterSettingResponse>.Failure(StatusCodes.Status400BadRequest, ErrorCodes.InvalidSettingTime, "생성 시간은 발송 시간보다 앞서야 합니다.");
        }

        NewsletterSetting setting = await FindSettingAsync(cancellationToken);
        setting.GenerateTime = parsedGenerateTime;
        setting.SendTime = parsedSendTime;
        setting.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<NewsletterSettingResponse>.Success(ToResponse(setting));
    }

    /// <summary>
    /// "HH:mm" 형식(00:00~23:59)만 받는다. "7:50", "07:50:00", "24:00"은 받지 않는다
    /// </summary>
    public static bool TryParseTime(string? value, out TimeOnly time)
    {
        return TimeOnly.TryParseExact(value, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out time);
    }

    private async Task<NewsletterSetting> FindSettingAsync(CancellationToken cancellationToken)
    {
        // database.md 4.6의 insert로 만든 1행. 없으면 설정이 잘못된 것이므로 오류로 처리한다 (500 INTERNAL_ERROR)
        return await _dbContext.NewsletterSettings.OrderBy(setting => setting.Id).FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("newsletter_settings 행이 없습니다.");
    }

    private static NewsletterSettingResponse ToResponse(NewsletterSetting setting)
    {
        return new NewsletterSettingResponse(
            setting.GenerateTime.ToString(TimeFormat, CultureInfo.InvariantCulture),
            setting.SendTime.ToString(TimeFormat, CultureInfo.InvariantCulture));
    }
}
