using Microsoft.EntityFrameworkCore;
using Newsletter.Data;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;

namespace Newsletter.Services;

/// <summary>
/// 수신자 관리 (D-016, D-076). 여러 명을 한 번에 추가/삭제하고, 하나라도 오류면 아무것도 바꾸지 않는다
/// </summary>
public class RecipientService : IRecipientService
{
    private readonly NewsletterDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public RecipientService(NewsletterDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public Task<List<MemberResponse>> GetMembersAsync(bool? isRecipient, CancellationToken cancellationToken)
    {
        IQueryable<User> users = _dbContext.Users;
        if (isRecipient.HasValue)
        {
            bool wantsRecipient = isRecipient.Value;
            users = users.Where(user => _dbContext.Recipients.Any(recipient => recipient.UserId == user.Id) == wantsRecipient);
        }

        return users
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .Select(user => new MemberResponse(
                user.Id,
                user.Name,
                user.Email,
                _dbContext.Recipients.Any(recipient => recipient.UserId == user.Id)))
            .ToListAsync(cancellationToken);
    }

    public Task<List<RecipientResponse>> GetRecipientsAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Recipients
            .OrderBy(recipient => recipient.User!.Name)
            .ThenBy(recipient => recipient.UserId)
            .Select(recipient => new RecipientResponse(recipient.UserId, recipient.User!.Name, recipient.User.Email))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<RecipientChangeResponse>> AddAsync(List<long>? userIds, CancellationToken cancellationToken)
    {
        ServiceError? idsError = ValidateIds(userIds);
        if (idsError is not null)
        {
            return ServiceResult<RecipientChangeResponse>.Failure(idsError);
        }

        List<long> ids = userIds!;
        int existingUserCount = await _dbContext.Users.CountAsync(user => ids.Contains(user.Id), cancellationToken);
        if (existingUserCount != ids.Count)
        {
            return ServiceResult<RecipientChangeResponse>.Failure(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "존재하지 않는 회원이 포함되어 있습니다.");
        }

        bool hasRecipient = await _dbContext.Recipients.AnyAsync(recipient => ids.Contains(recipient.UserId), cancellationToken);
        if (hasRecipient)
        {
            return AlreadyRecipientFailure();
        }

        DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        List<Recipient> recipients = ids
            .Select(userId => new Recipient { UserId = userId, CreatedAt = nowUtc, UpdatedAt = nowUtc })
            .ToList();
        _dbContext.Recipients.AddRange(recipients);

        try
        {
            // 한 번의 SaveChanges로 저장해 모두 추가되거나 아무도 추가되지 않게 한다
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // 검사 직후 다른 관리자가 같은 회원을 먼저 추가한 경우 (idx_recipients_user_id 유니크 위반)
            foreach (Recipient recipient in recipients)
            {
                _dbContext.Entry(recipient).State = EntityState.Detached;
            }

            return AlreadyRecipientFailure();
        }

        return ServiceResult<RecipientChangeResponse>.Success(new RecipientChangeResponse(recipients.Count));
    }

    public async Task<ServiceResult<RecipientChangeResponse>> DeleteAsync(List<long>? userIds, CancellationToken cancellationToken)
    {
        ServiceError? idsError = ValidateIds(userIds);
        if (idsError is not null)
        {
            return ServiceResult<RecipientChangeResponse>.Failure(idsError);
        }

        List<long> ids = userIds!;
        List<Recipient> recipients = await _dbContext.Recipients
            .Where(recipient => ids.Contains(recipient.UserId))
            .ToListAsync(cancellationToken);
        if (recipients.Count != ids.Count)
        {
            return ServiceResult<RecipientChangeResponse>.Failure(StatusCodes.Status404NotFound, ErrorCodes.NotFound, "수신자가 아닌 회원이 포함되어 있습니다.");
        }

        // 발송 기록(send_logs)은 users를 참조하므로 수신자를 지워도 남는다 (D-055)
        _dbContext.Recipients.RemoveRange(recipients);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<RecipientChangeResponse>.Success(new RecipientChangeResponse(recipients.Count));
    }

    private static ServiceError? ValidateIds(List<long>? userIds)
    {
        if (userIds is null || userIds.Count == 0)
        {
            return new ServiceError(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput, "회원을 1명 이상 선택해 주세요.");
        }

        if (userIds.Distinct().Count() != userIds.Count)
        {
            return new ServiceError(StatusCodes.Status400BadRequest, ErrorCodes.InvalidInput, "같은 회원이 중복으로 선택되었습니다.");
        }

        return null;
    }

    private static ServiceResult<RecipientChangeResponse> AlreadyRecipientFailure()
    {
        return ServiceResult<RecipientChangeResponse>.Failure(StatusCodes.Status409Conflict, ErrorCodes.AlreadyRecipient, "이미 수신자로 등록된 회원이 포함되어 있습니다.");
    }
}
