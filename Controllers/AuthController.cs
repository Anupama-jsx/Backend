using Backend.Models.Auth;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IRegistrationService registrationService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<RegisteredUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisteredUserResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await registrationService.RegisterAsync(request, cancellationToken);

        return result.Type switch
        {
            RegistrationResultType.Created => StatusCode(StatusCodes.Status201Created, result.User),
            RegistrationResultType.Conflict => Conflict(result.Error),
            RegistrationResultType.Invalid => ValidationProblem(new ValidationProblemDetails(result.ValidationErrors!)),
            _ => throw new InvalidOperationException("Unexpected registration result.")
        };
    }
}
