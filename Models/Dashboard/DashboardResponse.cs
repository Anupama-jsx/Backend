namespace Backend.Models.Dashboard;

public sealed record DashboardResponse(
    string DisplayName,
    DashboardStatisticsResponse Statistics,
    IReadOnlyList<RaceResultResponse> RecentRaces);

public sealed record DashboardStatisticsResponse(
    int RacesPlayed,
    int Wins,
    int Losses,
    int NoContests,
    double WinRatePercentage,
    decimal? BestWpm,
    decimal? AverageWpm,
    decimal? AverageAccuracy);

public sealed record RaceResultResponse(
    Guid RaceId,
    DateTime CompletedAtUtc,
    string Opponents,
    string Result,
    string? WinnerName,
    decimal? Wpm,
    decimal? AccuracyPercentage,
    long? ElapsedMilliseconds);
