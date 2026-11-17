using Backend.Models.Racing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public sealed class RaceConfiguration : IEntityTypeConfiguration<Race>
{
    public void Configure(EntityTypeBuilder<Race> entity)
    {
        entity.ToTable("Races", table =>
        {
            table.HasCheckConstraint("CK_Races_DurationSeconds", "[DurationSeconds] > 0");
            table.HasCheckConstraint("CK_Races_Status", "[Status] IN ('Waiting', 'Countdown', 'Running', 'Completed', 'Cancelled')");
            table.HasCheckConstraint("CK_Races_Timing", "[EndedAtUtc] IS NULL OR [StartsAtUtc] IS NULL OR [EndedAtUtc] >= [StartsAtUtc]");
        });
        entity.HasKey(race => race.Id);
        entity.Property(race => race.RoomCode).IsRequired().HasMaxLength(8);
        entity.HasIndex(race => race.RoomCode).IsUnique();
        entity.HasOne<Backend.Identity.ApplicationUser>().WithMany()
            .HasForeignKey(race => race.HostUserId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(race => race.Status).HasConversion<string>().HasMaxLength(16);
        entity.Property(race => race.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        entity.Property(race => race.RowVersion).IsRowVersion();
        entity.HasOne(race => race.Passage)
            .WithMany()
            .HasForeignKey(race => race.PassageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
