using Backend.Models.Auth;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IRegistrationService registrationService,
    ILoginService loginService) : ControllerBase
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

    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await loginService.LoginAsync(request, cancellationToken);

        return result.Type switch
        {
            LoginResultType.Success => Ok(result.Response),
            LoginResultType.Invalid => ValidationProblem(new ValidationProblemDetails(result.ValidationErrors!)),
            LoginResultType.Unauthorized => Unauthorized(result.Error),
            _ => throw new InvalidOperationException("Unexpected login result.")
        };
    }

    [HttpPost("refresh")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await loginService.RefreshAsync(request, cancellationToken);

        return result.Type switch
        {
            LoginResultType.Success => Ok(result.Response),
            LoginResultType.Invalid => ValidationProblem(new ValidationProblemDetails(result.ValidationErrors!)),
            LoginResultType.Unauthorized => Unauthorized(result.Error),
            _ => throw new InvalidOperationException("Unexpected refresh result.")
        };
    }
}
