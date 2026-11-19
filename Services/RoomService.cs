using System.Security.Cryptography;
using Backend.Data;
using Backend.Models.Racing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public sealed class RoomService(ApplicationDbContext db)
{
    public const int MaxParticipants = 2;
    public const int CountdownSeconds = 5;

    public async Task<StartRaceResult> StartAsync(
        Guid userId, Guid raceId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        // Use the same lock ordering as JoinAsync so that a start cannot race a join.
        var race = await db.Races
            .FromSqlInterpolated($"SELECT * FROM [Races] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {raceId}")
            .Include(r => r.Passage)
            .SingleOrDefaultAsync(cancellationToken);
        if (race is null) return new(StartRaceOutcome.NotFound);

        var participant = await db.RaceParticipants.AsNoTracking().SingleOrDefaultAsync(
            p => p.RaceId == raceId && p.UserId == userId, cancellationToken);
        if (participant is null || participant.Status == RaceParticipantStatus.Left)
            return new(StartRaceOutcome.NotParticipant);
        if (race.HostUserId != userId) return new(StartRaceOutcome.NotHost);

        if (race.Status is RaceStatus.Countdown or RaceStatus.Running)
        {
            await transaction.CommitAsync(cancellationToken);
            return new(StartRaceOutcome.AlreadyStarted, ToLiveResponse(race));
        }

        if (race.Status != RaceStatus.Waiting)
            return new(StartRaceOutcome.Unavailable);

        // StartsAtUtc is the moment typing becomes active, not the moment the host
        // pressed Start. Every client can calculate the same five-second countdown.
        race.Status = RaceStatus.Countdown;
        race.StartsAtUtc = DateTime.UtcNow.AddSeconds(CountdownSeconds);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(StartRaceOutcome.Started, ToLiveResponse(race));
    }

    public static RaceLiveResponse ToLiveResponse(Race race)
    {
        // SQL Server datetime2 does not retain DateTime.Kind. Restore the UTC
        // contract before serializing, otherwise browsers interpret it as local time.
        DateTime? startsAtUtc = race.StartsAtUtc is DateTime startsAt
            ? DateTime.SpecifyKind(startsAt, DateTimeKind.Utc)
            : null;
        DateTime? endedAtUtc = race.EndedAtUtc is DateTime endedAt
            ? DateTime.SpecifyKind(endedAt, DateTimeKind.Utc)
            : null;

        return new(
            race.Id,
            DateTime.UtcNow,
            race.Status.ToString(),
            startsAtUtc,
            endedAtUtc ?? startsAtUtc?.AddSeconds(race.DurationSeconds),
            race.DurationSeconds,
            race.Status == RaceStatus.Countdown ? CountdownSeconds : 0,
            race.Status is RaceStatus.Running or RaceStatus.Completed ? race.Passage.Content : null);
    }

    public async Task<JoinRoomResult> JoinAsync(
        Guid userId, string roomCode, CancellationToken cancellationToken)
    {
        var normalizedCode = roomCode.Trim().ToUpperInvariant();
        if (normalizedCode.Length != 8 || normalizedCode.Any(c => !char.IsAsciiLetterOrDigit(c)))
            return new(JoinRoomOutcome.InvalidCode);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        // Serialize joins to the same room across API instances. Start/leave operations
        // must also lock the race first, before inspecting or changing participants.
        var race = await db.Races.FromSqlInterpolated(
                $"SELECT * FROM [Races] WITH (UPDLOCK, HOLDLOCK) WHERE [RoomCode] = {normalizedCode}")
            .SingleOrDefaultAsync(cancellationToken);
        if (race is null) return new(JoinRoomOutcome.NotFound);
        if (race.HostUserId is null) return new(JoinRoomOutcome.Closed);

        var existing = await db.RaceParticipants.AsNoTracking()
            .SingleOrDefaultAsync(p => p.RaceId == race.Id && p.UserId == userId, cancellationToken);
        var response = new RoomResponse(race.Id, race.RoomCode, race.HostUserId.Value,
            race.Status.ToString(), race.DurationSeconds);

        // A retry reports the existing membership even if the room has since started.
        // A participant who left cannot rejoin through this operation.
        if (existing is not null)
            return existing.Status == RaceParticipantStatus.Left
                ? new(JoinRoomOutcome.Closed)
                : new(JoinRoomOutcome.Joined, response);

        if (race.Status != RaceStatus.Waiting) return new(JoinRoomOutcome.Closed);
        var count = await db.RaceParticipants.CountAsync(
            p => p.RaceId == race.Id && p.Status != RaceParticipantStatus.Left, cancellationToken);
        if (count >= MaxParticipants) return new(JoinRoomOutcome.Full);

        db.RaceParticipants.Add(new RaceParticipant { RaceId = race.Id, UserId = userId });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(JoinRoomOutcome.Joined, response);
    }

    public async Task<RoomResponse?> CreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var passageId = await db.Passages.OrderBy(_ => Guid.NewGuid())
            .Select(p => (Guid?)p.Id).FirstOrDefaultAsync(cancellationToken);
        if (passageId is null) return null;

        for (var attempt = 0; attempt < 5; attempt++)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var code = new string(Enumerable.Range(0, 8)
                .Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]).ToArray());
            var race = new Race
            {
                RoomCode = code, HostUserId = userId,
                PassageId = passageId.Value, DurationSeconds = 120
            };
            var participant = new RaceParticipant { RaceId = race.Id, UserId = userId };
            race.Participants.Add(participant);
            db.Races.Add(race);
            try
            {
                // One SaveChanges transaction inserts the race and its host participation.
                await db.SaveChangesAsync(cancellationToken);
                return new RoomResponse(race.Id, code, userId, race.Status.ToString(), race.DurationSeconds);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                db.Entry(participant).State = EntityState.Detached;
                db.Entry(race).State = EntityState.Detached;
                // Retry only an actual room-code collision; other uniqueness failures are bugs.
                if (!await db.Races.AnyAsync(r => r.RoomCode == code, cancellationToken)) throw;
            }
        }
        throw new InvalidOperationException("Unable to allocate a unique room code.");
    }
}
