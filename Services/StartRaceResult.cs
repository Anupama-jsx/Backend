using Backend.Models.Racing;

namespace Backend.Services;

public enum StartRaceOutcome
{
    Started,
    AlreadyStarted,
    NotFound,
    NotHost,
    NotParticipant,
    Unavailable
}

public sealed record StartRaceResult(StartRaceOutcome Outcome, RaceLiveResponse? Race = null);
