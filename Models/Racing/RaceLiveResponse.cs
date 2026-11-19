namespace Backend.Models.Racing;

/// <summary>
/// The authoritative race state sent to room members over SignalR.
/// Passage content is withheld while the room is still in the lobby.
/// </summary>
public sealed record RaceLiveResponse(
    Guid RaceId,
    DateTime ServerTimeUtc,
    string Status,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    int DurationSeconds,
    int CountdownSeconds,
    string? Passage);

public sealed record RaceProgressUpdate(
    int CorrectCharacters,
    int TotalCharacterAttempts,
    int CorrectCharacterAttempts);

public sealed record RaceProgressResponse(
    Guid RaceId,
    Guid UserId,
    string DisplayName,
    int CorrectCharacters,
    int PassageLength,
    decimal Wpm,
    decimal AccuracyPercentage);
