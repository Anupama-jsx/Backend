using System.Security.Cryptography;
using Backend.Data;
using Backend.Models.Racing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public sealed class RoomService(ApplicationDbContext db)
{
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
