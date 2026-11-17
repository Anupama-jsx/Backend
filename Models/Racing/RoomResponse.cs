namespace Backend.Models.Racing;

public sealed record RoomResponse(Guid RaceId, string RoomCode, Guid HostUserId,
    string Status, int DurationSeconds);
