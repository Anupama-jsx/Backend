using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Identity;
using Backend.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(UserManager<ApplicationUser> userManager) : ControllerBase
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

        return Ok(new DashboardResponse(
            user.UserName!,
            new DashboardStatisticsResponse(
                RacesPlayed: 0,
                Wins: 0,
                AverageWpm: null,
                AverageAccuracy: null)));
    }
}
