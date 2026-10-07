using Newsletter.Models.Dtos;

namespace Newsletter.Services;

public interface IRecipientService
{
    /// <summary>
    /// 회원 목록 (이름 순). isRecipient가 있으면 수신자 여부로 거른다
    /// </summary>
    Task<List<MemberResponse>> GetMembersAsync(bool? isRecipient, CancellationToken cancellationToken);

    /// <summary>
    /// 수신자 목록 (이름 순)
    /// </summary>
    Task<List<RecipientResponse>> GetRecipientsAsync(CancellationToken cancellationToken);

    Task<ServiceResult<RecipientChangeResponse>> AddAsync(List<long>? userIds, CancellationToken cancellationToken);

    Task<ServiceResult<RecipientChangeResponse>> DeleteAsync(List<long>? userIds, CancellationToken cancellationToken);
}
