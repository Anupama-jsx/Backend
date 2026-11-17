using Backend.Models.Racing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public sealed class RaceParticipantConfiguration : IEntityTypeConfiguration<RaceParticipant>
{
    public void Configure(EntityTypeBuilder<RaceParticipant> entity)
    {
        entity.ToTable("RaceParticipants", table =>
        {
            table.HasCheckConstraint("CK_RaceParticipants_Status", "[Status] IN ('Joined', 'Finished', 'TimedOut', 'Left')");
            table.HasCheckConstraint("CK_RaceParticipants_Elapsed", "[ElapsedMilliseconds] IS NULL OR [ElapsedMilliseconds] >= 0");
            table.HasCheckConstraint("CK_RaceParticipants_Wpm", "[Wpm] IS NULL OR [Wpm] >= 0");
            table.HasCheckConstraint("CK_RaceParticipants_Accuracy", "[AccuracyPercentage] IS NULL OR [AccuracyPercentage] BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_RaceParticipants_Attempts", "([TotalCharacterAttempts] IS NULL OR [TotalCharacterAttempts] >= 0) AND ([CorrectCharacterAttempts] IS NULL OR [CorrectCharacterAttempts] >= 0) AND ([CorrectCharacterAttempts] IS NULL OR [TotalCharacterAttempts] IS NULL OR [CorrectCharacterAttempts] <= [TotalCharacterAttempts])");
            table.HasCheckConstraint("CK_RaceParticipants_FinalCorrectCharacters", "[FinalCorrectCharacters] IS NULL OR [FinalCorrectCharacters] >= 0");
        });
        entity.HasKey(participant => participant.Id);
        entity.Property(participant => participant.Status).HasConversion<string>().HasMaxLength(16);
        entity.Property(participant => participant.JoinedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        entity.Property(participant => participant.Wpm).HasPrecision(12, 2);
        entity.Property(participant => participant.AccuracyPercentage).HasPrecision(5, 2);
        entity.Property(participant => participant.RowVersion).IsRowVersion();
        entity.HasIndex(participant => new { participant.RaceId, participant.UserId }).IsUnique();
        entity.HasOne(participant => participant.Race)
            .WithMany(race => race.Participants)
            .HasForeignKey(participant => participant.RaceId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(participant => participant.User)
            .WithMany()
            .HasForeignKey(participant => participant.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
