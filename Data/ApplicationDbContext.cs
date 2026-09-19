using Backend.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(user => user.Email).IsRequired().HasMaxLength(256);
            entity.Property(user => user.NormalizedEmail).IsRequired().HasMaxLength(256);
            entity.Property(user => user.UserName).IsRequired().HasMaxLength(20);
            entity.Property(user => user.NormalizedUserName).IsRequired().HasMaxLength(20);
            entity.Property(user => user.CreatedAtUtc)
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(user => user.Status)
                .HasConversion<string>()
                .HasMaxLength(16)
                .HasDefaultValue(UserStatus.Active);

            entity.HasIndex(user => user.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("UX_Users_NormalizedEmail");
            entity.HasIndex(user => user.NormalizedUserName)
                .IsUnique()
                .HasDatabaseName("UX_Users_NormalizedUsername");
        });

        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).IsRequired().HasMaxLength(64);
            entity.Property(token => token.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(token => token.RowVersion).IsRowVersion();
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UserId, token.ExpiresAtUtc });
            entity.HasOne(token => token.User)
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
