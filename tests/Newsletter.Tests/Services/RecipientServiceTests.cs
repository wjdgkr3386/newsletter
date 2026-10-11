using Newsletter.Data;
using Newsletter.Models.Dtos;
using Newsletter.Models.Entities;
using Newsletter.Services;
using Newsletter.Tests.TestHelpers;

namespace Newsletter.Tests.Services;

/// <summary>
/// 수신자 관리 (api.md 6.2, D-076) 테스트
/// </summary>
public class RecipientServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 7, 10, 0, 0, DateTimeKind.Utc);

    private readonly NewsletterDbContext _dbContext = TestDbContextFactory.Create();
    private readonly RecipientService _service;

    public RecipientServiceTests()
    {
        _service = new RecipientService(_dbContext, new FakeTimeProvider(Now));
    }

    private async Task<long> AddUserAsync(string loginId, string name, bool isRecipient)
    {
        User user = new() { LoginId = loginId, PasswordHash = "hash", Email = loginId + "@gmail.com", Name = name };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (isRecipient)
        {
            _dbContext.Recipients.Add(new Recipient { UserId = user.Id, CreatedAt = Now, UpdatedAt = Now });
            await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        return user.Id;
    }

    // ---------- 목록 ----------
    [Fact]
    public async Task GetMembers_FilterByRecipient_ReturnsMatchingMembersByName()
    {
        await AddUserAsync("user0003", "홍길동", true);
        await AddUserAsync("user0001", "김민수", false);
        await AddUserAsync("user0002", "박영희", true);

        List<MemberResponse> all = await _service.GetMembersAsync(null, CancellationToken.None);
        List<MemberResponse> nonRecipients = await _service.GetMembersAsync(false, CancellationToken.None);
        List<MemberResponse> onlyRecipients = await _service.GetMembersAsync(true, CancellationToken.None);

        Assert.Equal(["김민수", "박영희", "홍길동"], all.Select(member => member.Name));
        Assert.Equal(["김민수"], nonRecipients.Select(member => member.Name));
        Assert.Equal(["박영희", "홍길동"], onlyRecipients.Select(member => member.Name));
        Assert.All(onlyRecipients, member => Assert.True(member.IsRecipient));
    }

    [Fact]
    public async Task GetRecipients_ReturnsRecipientsByNameWithEmail()
    {
        await AddUserAsync("user0002", "홍길동", true);
        await AddUserAsync("user0001", "김민수", true);
        await AddUserAsync("user0003", "박영희", false);

        List<RecipientResponse> recipients = await _service.GetRecipientsAsync(CancellationToken.None);

        Assert.Equal(["김민수", "홍길동"], recipients.Select(recipient => recipient.Name));
        Assert.Equal("user0001@gmail.com", recipients[0].Email);
    }

    [Fact]
    public async Task GetRecipients_NoRecipient_ReturnsEmpty()
    {
        await AddUserAsync("user0001", "김민수", false);

        Assert.Empty(await _service.GetRecipientsAsync(CancellationToken.None));
    }

    // ---------- 추가 ----------
    [Fact]
    public async Task Add_OneMember_AddsRecipient()
    {
        long userId = await AddUserAsync("user0001", "김민수", false);

        ServiceResult<RecipientChangeResponse> result = await _service.AddAsync([userId], CancellationToken.None);

        Assert.Equal(1, result.Data!.Count);
        Recipient saved = Assert.Single(_dbContext.Recipients);
        Assert.Equal(userId, saved.UserId);
        Assert.Equal(Now, saved.CreatedAt);
    }

    [Fact]
    public async Task Add_ManyMembers_AddsAll()
    {
        long firstId = await AddUserAsync("user0001", "김민수", false);
        long secondId = await AddUserAsync("user0002", "박영희", false);
        long thirdId = await AddUserAsync("user0003", "홍길동", false);

        ServiceResult<RecipientChangeResponse> result = await _service.AddAsync([firstId, secondId, thirdId], CancellationToken.None);

        Assert.Equal(3, result.Data!.Count);
        Assert.Equal(3, _dbContext.Recipients.Count());
    }

    [Fact]
    public async Task Add_IncludesAlreadyRecipient_AddsNobody()
    {
        long newId = await AddUserAsync("user0001", "김민수", false);
        long recipientId = await AddUserAsync("user0002", "박영희", true);

        ServiceResult<RecipientChangeResponse> result = await _service.AddAsync([newId, recipientId], CancellationToken.None);

        Assert.Equal(ErrorCodes.AlreadyRecipient, result.Error!.Code);
        Assert.Equal(409, result.Error.StatusCode);
        Assert.Single(_dbContext.Recipients); // 일부만 추가되지 않는다
    }

    [Fact]
    public async Task Add_IncludesUnknownMember_AddsNobody()
    {
        long userId = await AddUserAsync("user0001", "김민수", false);

        ServiceResult<RecipientChangeResponse> result = await _service.AddAsync([userId, 9999], CancellationToken.None);

        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
        Assert.Empty(_dbContext.Recipients);
    }

    [Theory]
    [InlineData(false)] // null
    [InlineData(true)] // 빈 목록 (경계: 0명)
    public async Task Add_NoIds_ReturnsInvalidInput(bool isEmptyList)
    {
        List<long>? userIds = isEmptyList ? [] : null;

        ServiceResult<RecipientChangeResponse> result = await _service.AddAsync(userIds, CancellationToken.None);

        Assert.Equal(ErrorCodes.InvalidInput, result.Error!.Code);
    }

    [Fact]
    public async Task Add_DuplicateIds_ReturnsInvalidInput()
    {
        long userId = await AddUserAsync("user0001", "김민수", false);

        ServiceResult<RecipientChangeResponse> result = await _service.AddAsync([userId, userId], CancellationToken.None);

        Assert.Equal(ErrorCodes.InvalidInput, result.Error!.Code);
        Assert.Empty(_dbContext.Recipients);
    }

    // ---------- 삭제 ----------
    [Fact]
    public async Task Delete_OneRecipient_RemovesRecipientOnly()
    {
        long userId = await AddUserAsync("user0001", "김민수", true);

        ServiceResult<RecipientChangeResponse> result = await _service.DeleteAsync([userId], CancellationToken.None);

        Assert.Equal(1, result.Data!.Count);
        Assert.Empty(_dbContext.Recipients);
        Assert.Single(_dbContext.Users); // 회원은 남는다
    }

    [Fact]
    public async Task Delete_ManyRecipients_RemovesAll()
    {
        long firstId = await AddUserAsync("user0001", "김민수", true);
        long secondId = await AddUserAsync("user0002", "박영희", true);
        long keptId = await AddUserAsync("user0003", "홍길동", true);

        ServiceResult<RecipientChangeResponse> result = await _service.DeleteAsync([firstId, secondId], CancellationToken.None);

        Assert.Equal(2, result.Data!.Count);
        Assert.Equal(keptId, Assert.Single(_dbContext.Recipients).UserId);
    }

    [Fact]
    public async Task Delete_IncludesNonRecipient_DeletesNobody()
    {
        long recipientId = await AddUserAsync("user0001", "김민수", true);
        long memberId = await AddUserAsync("user0002", "박영희", false);

        ServiceResult<RecipientChangeResponse> result = await _service.DeleteAsync([recipientId, memberId], CancellationToken.None);

        Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
        Assert.Single(_dbContext.Recipients); // 일부만 삭제되지 않는다
    }

    [Fact]
    public async Task Delete_EmptyIds_ReturnsInvalidInput()
    {
        ServiceResult<RecipientChangeResponse> result = await _service.DeleteAsync([], CancellationToken.None);

        Assert.Equal(ErrorCodes.InvalidInput, result.Error!.Code);
    }

    [Fact]
    public async Task Delete_DuplicateIds_ReturnsInvalidInput()
    {
        long userId = await AddUserAsync("user0001", "김민수", true);

        ServiceResult<RecipientChangeResponse> result = await _service.DeleteAsync([userId, userId], CancellationToken.None);

        Assert.Equal(ErrorCodes.InvalidInput, result.Error!.Code);
        Assert.Single(_dbContext.Recipients);
    }
}
