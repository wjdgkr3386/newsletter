namespace Newsletter.Models.Entities;

/// <summary>
/// newsletter_settings 테이블 (뉴스레터 설정, 1행만 쓴다)
/// </summary>
public class NewsletterSetting
{
    public long Id { get; set; }

    /// <summary>
    /// 자동 생성 시각 (KST, 분 단위)
    /// </summary>
    public TimeOnly GenerateTime { get; set; }

    /// <summary>
    /// 자동 발송 시각 (KST, 분 단위)
    /// </summary>
    public TimeOnly SendTime { get; set; }

    /// <summary>
    /// 자동 생성을 마지막으로 실행한 날짜 (KST)
    /// </summary>
    public DateOnly? LastGenerateDate { get; set; }

    /// <summary>
    /// 자동 발송을 마지막으로 실행한 날짜 (KST)
    /// </summary>
    public DateOnly? LastSendDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
