namespace Backend.Models.Racing;

public sealed class Race
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RoomCode { get; set; } = null!;
    public Guid? HostUserId { get; set; }
    public Guid PassageId { get; set; }
    public RaceStatus Status { get; set; } = RaceStatus.Waiting;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public int DurationSeconds { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public Passage Passage { get; set; } = null!;
    public ICollection<RaceParticipant> Participants { get; set; } = new List<RaceParticipant>();
}
