using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Identity;
using Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Backend.Models.Racing;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/rooms")]
public sealed class RoomsController(RoomService rooms, UserManager<ApplicationUser> users,
    IHubContext<RaceHub> hub, ILogger<RoomsController> logger) : ControllerBase
{
    [HttpPost("{raceId:guid}/start")]
    [ProducesResponseType<RaceLiveResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RaceLiveResponse>> Start(Guid raceId, CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(claim, out var userId)) return Unauthorized();
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || user.Status != UserStatus.Active) return Unauthorized();

        var result = await rooms.StartAsync(userId, raceId, cancellationToken);
        if (result.Outcome == StartRaceOutcome.Started && result.Race is not null)
        {
            try
            {
                await hub.Clients.Group(RaceHub.RoomGroup(raceId))
                    .SendAsync("RaceCountdownStarted", result.Race, cancellationToken);
            }
            catch (Exception exception)
            {
                // Starting is committed before notifying clients; they can fetch the snapshot on reconnect.
                logger.LogWarning(exception, "Race-start notification failed for race {RaceId}", raceId);
            }
        }

        return result.Outcome switch
        {
            StartRaceOutcome.Started or StartRaceOutcome.AlreadyStarted => Ok(result.Race),
            StartRaceOutcome.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Room not found."),
            StartRaceOutcome.NotHost => Forbid(),
            StartRaceOutcome.NotParticipant => Forbid(),
            _ => Problem(statusCode: StatusCodes.Status409Conflict, title: "This race cannot be started.")
        };
    }

    [HttpPost("join")]
    [ProducesResponseType<RoomResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomResponse>> Join(
        JoinRoomRequest request, CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(claim, out var userId)) return Unauthorized();
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || user.Status != UserStatus.Active) return Unauthorized();

        var result = await rooms.JoinAsync(userId, request.RoomCode, cancellationToken);
        if (result.Outcome == JoinRoomOutcome.Joined && result.Room is not null)
        {
            try
            {
                // JoinAsync has committed membership before this notification.
                // Send only an invalidation; each authorized client reads a fresh list.
                await hub.Clients.Group(RaceHub.RoomGroup(result.Room.RaceId))
                    .SendAsync("LobbyChanged", result.Room.RaceId);
            }
            catch (Exception exception)
            {
                // A notification failure must not report a committed join as failed.
                logger.LogWarning(exception, "Lobby notification failed for race {RaceId}", result.Room.RaceId);
            }
        }
        return result.Outcome switch
        {
            JoinRoomOutcome.Joined => Ok(result.Room),
            JoinRoomOutcome.InvalidCode => Problem(statusCode: 400,
                title: "Enter an eight-character room code containing letters and numbers."),
            JoinRoomOutcome.NotFound => Problem(statusCode: 404, title: "Room not found."),
            JoinRoomOutcome.Full => Problem(statusCode: 409, title: "Room is full."),
            JoinRoomOutcome.Closed => Problem(statusCode: 409, title: "Room is not accepting joins."),
            _ => throw new InvalidOperationException("Unexpected join outcome.")
        };
    }

    [HttpPost]
    [ProducesResponseType<RoomResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<RoomResponse>> Create(CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(claim, out var userId)) return Unauthorized();
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || user.Status != UserStatus.Active) return Unauthorized();

        var room = await rooms.CreateAsync(userId, cancellationToken);
        if (room is null)
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "No passages are available. Apply the passage seed migration first.");
        return StatusCode(StatusCodes.Status201Created, room);
    }
}
