using Backend.Identity;

namespace Backend.Models.Racing;

public sealed class RaceParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RaceId { get; set; }
    public Guid UserId { get; set; }
    public RaceParticipantStatus Status { get; set; } = RaceParticipantStatus.Joined;
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAtUtc { get; set; }
    public long? ElapsedMilliseconds { get; set; }
    public decimal? Wpm { get; set; }
    public decimal? AccuracyPercentage { get; set; }
    public int? TotalCharacterAttempts { get; set; }
    public int? CorrectCharacterAttempts { get; set; }
    public int? FinalCorrectCharacters { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public Race Race { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
