using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newsletter.Models.Entities;

namespace Newsletter.Data;

/// <summary>
/// newsletter DB 매핑. 테이블은 database.md SQL로 만들고 마이그레이션은 쓰지 않는다 (D-059)
/// </summary>
public class NewsletterDbContext : DbContext
{
    // DB의 datetime2 값은 UTC로 저장되므로 읽을 때 Kind를 UTC로 지정한다
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public NewsletterDbContext(DbContextOptions<NewsletterDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();

    public DbSet<Recipient> Recipients => Set<Recipient>();

    public DbSet<NewsletterSetting> NewsletterSettings => Set<NewsletterSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.LoginId).HasColumnName("login_id").HasColumnType("varchar(20)").HasMaxLength(20);
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash").HasColumnType("char(60)").HasMaxLength(60);
            entity.Property(user => user.Email).HasColumnName("email").HasColumnType("varchar(50)").HasMaxLength(50);
            entity.Property(user => user.Name).HasColumnName("name").HasColumnType("nvarchar(30)").HasMaxLength(30);
            entity.Property(user => user.Role).HasColumnName("role").HasColumnType("varchar(10)").HasMaxLength(10);
            entity.Property(user => user.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2(0)").HasConversion(UtcConverter);
            entity.Property(user => user.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2(0)").HasConversion(UtcConverter);
            entity.HasIndex(user => user.LoginId).IsUnique().HasDatabaseName("idx_users_login_id");
            entity.HasIndex(user => user.Email).IsUnique().HasDatabaseName("idx_users_email");
        });

        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.ToTable("email_verifications");
            entity.HasKey(verification => verification.Id);
            entity.Property(verification => verification.Id).HasColumnName("id");
            entity.Property(verification => verification.Email).HasColumnName("email").HasColumnType("varchar(50)").HasMaxLength(50);
            entity.Property(verification => verification.Code).HasColumnName("code").HasColumnType("char(6)").HasMaxLength(6);
            entity.Property(verification => verification.ExpiresAt).HasColumnName("expires_at").HasColumnType("datetime2(0)").HasConversion(UtcConverter);
            entity.Property(verification => verification.IsVerified).HasColumnName("is_verified");
            entity.Property(verification => verification.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2(0)").HasConversion(UtcConverter);
            entity.Property(verification => verification.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2(0)").HasConversion(UtcConverter);
            entity.Property(verification => verification.FailCount).HasColumnName("fail_count");
            entity.HasIndex(verification => verification.Email).HasDatabaseName("idx_email_verifications_email");
        });

        modelBuilder.Entity<Recipient>(entity =>
        {
            entity.ToTable("recipients");
            entity.HasKey(recipient => recipient.Id);
            entity.Property(recipient => recipient.Id).HasColumnName("id");
            entity.Property(recipient => recipient.UserId).HasColumnName("user_id");
            entity.Property(recipient => recipient.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2(0)").HasConversion(UtcConverter);
            entity.Property(recipient => recipient.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2(0)").HasConversion(UtcConverter);
            entity.HasIndex(recipient => recipient.UserId).IsUnique().HasDatabaseName("idx_recipients_user_id");
            entity.HasOne(recipient => recipient.User)
                .WithMany()
                .HasForeignKey(recipient => recipient.UserId)
                .HasConstraintName("fk_recipients_user_id");
        });

        modelBuilder.Entity<NewsletterSetting>(entity =>
        {
            entity.ToTable("newsletter_settings");
            entity.HasKey(setting => setting.Id);
            entity.Property(setting => setting.Id).HasColumnName("id");
            entity.Property(setting => setting.GenerateTime).HasColumnName("generate_time").HasColumnType("time(0)");
            entity.Property(setting => setting.SendTime).HasColumnName("send_time").HasColumnType("time(0)");
            entity.Property(setting => setting.LastGenerateDate).HasColumnName("last_generate_date").HasColumnType("date");
            entity.Property(setting => setting.LastSendDate).HasColumnName("last_send_date").HasColumnType("date");
            entity.Property(setting => setting.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2(0)").HasConversion(UtcConverter);
            entity.Property(setting => setting.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2(0)").HasConversion(UtcConverter);
        });
    }
}
