using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Backend.Data;
using Backend.Identity;
using Backend.Models.Racing;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Hubs;

[Authorize]
public sealed class RaceHub(ApplicationDbContext db, RoomService rooms) : Hub
{
    public static string RoomGroup(Guid raceId) => $"race:{raceId:D}";

    private async Task<Guid> RequireMembership(Guid raceId)
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(claim, out var userId) ||
            !await db.RaceParticipants.AsNoTracking().AnyAsync(p =>
                p.RaceId == raceId && p.UserId == userId &&
                p.Status != RaceParticipantStatus.Left && p.User.Status == UserStatus.Active,
                Context.ConnectionAborted))
            throw new HubException("You are not a member of this room.");
        return userId;
    }

    public async Task SubscribeToRoom(Guid raceId)
    {
        await RequireMembership(raceId);
        // Subscribe before reading the snapshot, so joins in between are not missed.
        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(raceId), Context.ConnectionAborted);
    }

    public async Task<LobbyResponse> GetLobby(Guid raceId)
    {
        var userId = await RequireMembership(raceId);
        var race = await db.Races.AsNoTracking().SingleAsync(r => r.Id == raceId, Context.ConnectionAborted);
        var participants = await db.RaceParticipants.AsNoTracking()
            .Where(p => p.RaceId == raceId && p.Status != RaceParticipantStatus.Left)
            .OrderBy(p => p.JoinedAtUtc).ThenBy(p => p.UserId)
            .Select(p => new LobbyParticipantResponse(p.UserId, p.User.UserName!))
            .ToListAsync(Context.ConnectionAborted);
        return new LobbyResponse(race.Id, race.HostUserId, userId, RoomService.MaxParticipants, participants);
    }

    public async Task<RaceLiveResponse> GetRace(Guid raceId)
    {
        await RequireMembership(raceId);
        var race = await db.Races.AsNoTracking()
            .Include(r => r.Passage)
            .SingleOrDefaultAsync(r => r.Id == raceId, Context.ConnectionAborted)
            ?? throw new HubException("Room not found.");
        return RoomService.ToLiveResponse(race);
    }

    public async Task<RaceLiveResponse> StartRace(Guid raceId)
    {
        var userId = await RequireMembership(raceId);
        // A host may start from a newly reconnected tab without first subscribing.
        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(raceId), Context.ConnectionAborted);

        var result = await rooms.StartAsync(userId, raceId, Context.ConnectionAborted);
        var race = result.Race;
        switch (result.Outcome)
        {
            case StartRaceOutcome.Started:
                await Clients.Group(RoomGroup(raceId)).SendAsync("RaceCountdownStarted", race!, Context.ConnectionAborted);
                return race!;
            case StartRaceOutcome.AlreadyStarted:
                return race!;
            case StartRaceOutcome.NotHost:
                throw new HubException("Only the room host can start the race.");
            case StartRaceOutcome.NotParticipant:
                throw new HubException("You are not a member of this room.");
            case StartRaceOutcome.NotFound:
                throw new HubException("Room not found.");
            default:
                throw new HubException("This race cannot be started.");
        }
    }

    public async Task ReportProgress(Guid raceId, RaceProgressUpdate update)
    {
        if (update is null) throw new HubException("Progress is required.");

        var userId = await RequireMembership(raceId);
        var race = await db.Races.AsNoTracking()
            .Where(r => r.Id == raceId)
            .Select(r => new { r.Status, r.StartsAtUtc, r.DurationSeconds, Passage = r.Passage.Content })
            .SingleOrDefaultAsync(Context.ConnectionAborted)
            ?? throw new HubException("Room not found.");
        if (race.Status != RaceStatus.Running || race.StartsAtUtc is null ||
            DateTime.UtcNow >= race.StartsAtUtc.Value.AddSeconds(race.DurationSeconds))
            throw new HubException("The race is not running.");
        var passageLength = race.Passage.Length;
        if (update.CorrectCharacters < 0 || update.CorrectCharacters > passageLength)
            throw new HubException("Progress is outside the passage bounds.");
        if (update.TotalCharacterAttempts < 0 || update.CorrectCharacterAttempts < 0 ||
            update.CorrectCharacterAttempts > update.TotalCharacterAttempts)
            throw new HubException("Attempt counts are invalid.");

        var elapsedMilliseconds = Math.Max(1L,
            (long)(DateTime.UtcNow - race.StartsAtUtc.Value).TotalMilliseconds);
        var wpm = decimal.Round(update.CorrectCharacters * 12_000m / elapsedMilliseconds, 2);
        var accuracy = update.TotalCharacterAttempts == 0
            ? 100m
            : decimal.Round(update.CorrectCharacterAttempts * 100m / update.TotalCharacterAttempts, 2);

        var participant = await db.RaceParticipants.SingleOrDefaultAsync(
            p => p.RaceId == raceId && p.UserId == userId && p.Status == RaceParticipantStatus.Joined,
            Context.ConnectionAborted);
        if (participant is null) throw new HubException("You cannot report progress for this race.");
        participant.ElapsedMilliseconds = elapsedMilliseconds;
        participant.Wpm = wpm;
        participant.AccuracyPercentage = accuracy;
        participant.TotalCharacterAttempts = update.TotalCharacterAttempts;
        participant.CorrectCharacterAttempts = update.CorrectCharacterAttempts;
        participant.FinalCorrectCharacters = update.CorrectCharacters;
        await db.SaveChangesAsync(Context.ConnectionAborted);

        var progress = new RaceProgressResponse(
            raceId, userId, update.CorrectCharacters, passageLength, wpm, accuracy);
        await Clients.Group(RoomGroup(raceId)).SendAsync("ParticipantProgress", progress, Context.ConnectionAborted);
    }
}
