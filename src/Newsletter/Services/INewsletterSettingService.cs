using Newsletter.Models.Dtos;

namespace Newsletter.Services;

public interface INewsletterSettingService
{
    Task<NewsletterSettingResponse> GetAsync(CancellationToken cancellationToken);

    Task<ServiceResult<NewsletterSettingResponse>> SaveAsync(string? generateTime, string? sendTime, CancellationToken cancellationToken);
}
