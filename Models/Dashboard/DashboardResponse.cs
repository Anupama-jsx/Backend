namespace Backend.Models.Dashboard;

public sealed record DashboardResponse(
    string DisplayName,
    DashboardStatisticsResponse Statistics);

public sealed record DashboardStatisticsResponse(
    int RacesPlayed,
    int Wins,
    double? AverageWpm,
    double? AverageAccuracy);
