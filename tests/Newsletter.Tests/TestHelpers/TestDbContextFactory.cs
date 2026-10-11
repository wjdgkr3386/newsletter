using Microsoft.EntityFrameworkCore;
using Newsletter.Data;

namespace Newsletter.Tests.TestHelpers;

/// <summary>
/// 테스트마다 비어 있는 메모리 DB를 만든다 (실제 SQL Server를 쓰지 않음)
/// </summary>
public static class TestDbContextFactory
{
    public static NewsletterDbContext Create()
    {
        DbContextOptions<NewsletterDbContext> options = new DbContextOptionsBuilder<NewsletterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NewsletterDbContext(options);
    }
}
