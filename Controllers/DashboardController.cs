using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Identity;
using Backend.Data;
using Backend.Models.Dashboard;
using Backend.Models.Racing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<DashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardResponse>> Get(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(parsedUserId.ToString());
        if (user is null || user.Status != UserStatus.Active)
        {
            return Unauthorized();
        }

        var raceIds = await db.RaceParticipants.AsNoTracking()
            .Where(p => p.UserId == parsedUserId && p.Status != RaceParticipantStatus.Left &&
                        p.Race.Status == RaceStatus.Completed)
            .Select(p => p.RaceId)
            .ToListAsync(cancellationToken);
        if (raceIds.Count == 0)
        {
            return Ok(new DashboardResponse(user.UserName!, EmptyStatistics(), []));
        }

        var races = await db.Races.AsNoTracking()
            .Where(r => raceIds.Contains(r.Id))
            .Select(r => new { r.Id, r.EndedAtUtc, r.StartsAtUtc })
            .ToListAsync(cancellationToken);
        var participants = await db.RaceParticipants.AsNoTracking()
            .Where(p => raceIds.Contains(p.RaceId) && p.Status != RaceParticipantStatus.Left)
            .Select(p => new
            {
                p.RaceId,
                p.UserId,
                DisplayName = p.User.UserName!,
                p.Status,
                p.FinishedAtUtc,
                p.Wpm,
                p.AccuracyPercentage,
                p.ElapsedMilliseconds
            })
            .ToListAsync(cancellationToken);

        var results = races.Select(race =>
        {
            var racers = participants.Where(p => p.RaceId == race.Id).ToList();
            var current = racers.Single(p => p.UserId == parsedUserId);
            var winner = racers
                .Where(p => p.Status == RaceParticipantStatus.Finished && p.FinishedAtUtc is not null)
                .OrderBy(p => p.FinishedAtUtc)
                .ThenBy(p => p.ElapsedMilliseconds)
                .FirstOrDefault();
            var result = winner is null ? "No contest" : winner.UserId == parsedUserId ? "Won" : "Lost";
            var completedAt = race.EndedAtUtc ?? race.StartsAtUtc ?? DateTime.UtcNow;
            return new RaceResultResponse(
                race.Id,
                DateTime.SpecifyKind(completedAt, DateTimeKind.Utc),
                string.Join(", ", racers.Where(p => p.UserId != parsedUserId).Select(p => p.DisplayName)),
                result,
                winner?.DisplayName,
                current.Wpm,
                current.AccuracyPercentage,
                current.ElapsedMilliseconds);
        })
        .OrderByDescending(result => result.CompletedAtUtc)
        .ToList();

        var measuredResults = results.Where(result => result.Wpm is not null).ToList();
        var wins = results.Count(result => result.Result == "Won");
        var losses = results.Count(result => result.Result == "Lost");
        var statistics = new DashboardStatisticsResponse(
            results.Count,
            wins,
            losses,
            results.Count(result => result.Result == "No contest"),
            results.Count == 0 ? 0 : Math.Round(wins * 100d / results.Count, 1),
            measuredResults.Count == 0 ? null : measuredResults.Max(result => result.Wpm),
            measuredResults.Count == 0 ? null : decimal.Round(measuredResults.Average(result => result.Wpm!.Value), 2),
            measuredResults.Count == 0 ? null : decimal.Round(measuredResults.Average(result => result.AccuracyPercentage ?? 0), 2));

        return Ok(new DashboardResponse(user.UserName!, statistics, results.Take(10).ToList()));
    }

    private static DashboardStatisticsResponse EmptyStatistics() => new(0, 0, 0, 0, 0, null, null, null);
}
