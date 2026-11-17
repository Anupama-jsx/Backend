using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Identity;
using Backend.Models.Racing;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/rooms")]
public sealed class RoomsController(RoomService rooms, UserManager<ApplicationUser> users) : ControllerBase
{
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
