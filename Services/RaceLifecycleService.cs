using Backend.Data;
using Backend.Hubs;
using Backend.Models.Racing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// Advances scheduled starts independently of connected clients. This keeps the
/// countdown authoritative and means a reconnect cannot delay the race for others.
/// </summary>
public sealed class RaceLifecycleService(
    IServiceScopeFactory scopeFactory,
    IHubContext<RaceHub> hub,
    ILogger<RaceLifecycleService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
        do
        {
            try
            {
                await AdvanceRacesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to advance scheduled races.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task AdvanceRacesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow;
        var readyToStart = await db.Races.AsNoTracking()
            .Where(r => r.Status == RaceStatus.Countdown && r.StartsAtUtc <= now)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        foreach (var raceId in readyToStart)
        {
            var updated = await db.Races
                .Where(r => r.Id == raceId && r.Status == RaceStatus.Countdown && r.StartsAtUtc <= now)
                .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.Status, RaceStatus.Running), cancellationToken);
            if (updated != 1) continue;

            var race = await db.Races.AsNoTracking().Include(r => r.Passage)
                .SingleAsync(r => r.Id == raceId, cancellationToken);
            await hub.Clients.Group(RaceHub.RoomGroup(raceId))
                .SendAsync("RaceStarted", RoomService.ToLiveResponse(race), cancellationToken);
        }

        var runningRaces = await db.Races.AsNoTracking()
            .Where(r => r.Status == RaceStatus.Running && r.StartsAtUtc != null)
            .Select(r => new { r.Id, r.StartsAtUtc, r.DurationSeconds })
            .ToListAsync(cancellationToken);
        foreach (var runningRace in runningRaces.Where(r =>
                     r.StartsAtUtc!.Value.AddSeconds(r.DurationSeconds) <= now))
        {
            var updated = await db.Races
                .Where(r => r.Id == runningRace.Id && r.Status == RaceStatus.Running)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.Status, RaceStatus.Completed)
                    .SetProperty(r => r.EndedAtUtc, now), cancellationToken);
            if (updated != 1) continue;

            var race = await db.Races.AsNoTracking().Include(r => r.Passage)
                .SingleAsync(r => r.Id == runningRace.Id, cancellationToken);
            await hub.Clients.Group(RaceHub.RoomGroup(race.Id))
                .SendAsync("RaceCompleted", RoomService.ToLiveResponse(race), cancellationToken);
        }
    }
}
