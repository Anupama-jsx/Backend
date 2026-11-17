using Backend.Models.Racing;

namespace Backend.Services;

public enum JoinRoomOutcome
{
    Joined,
    InvalidCode,
    NotFound,
    Closed,
    Full
}

public sealed record JoinRoomResult(JoinRoomOutcome Outcome, RoomResponse? Room = null);
