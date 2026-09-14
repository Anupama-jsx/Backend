using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Backend.Data;
using Backend.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Endpoints;

public static partial class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Creates a TypeRush account")
            .WithDescription("Creates an active, unverified account. This endpoint never signs the user in or returns credentials.")
            .Produces<RegisteredUserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count != 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        var email = request.Email!.Trim();
        var username = request.Username!.Trim();
        var normalizedEmail = userManager.NormalizeEmail(email)!;
        var normalizedUsername = userManager.NormalizeName(username)!;

        // These checks provide specific, friendly errors. The unique database indexes
        // below remain the authority when registration requests race each other.
        if (await dbContext.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            return Results.Conflict(new ApiErrorResponse("email_in_use", "An account already uses this email address."));
        }

        if (await dbContext.Users.AnyAsync(user => user.NormalizedUserName == normalizedUsername, cancellationToken))
        {
            return Results.Conflict(new ApiErrorResponse("username_in_use", "This username is unavailable."));
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = username,
            CreatedAtUtc = DateTime.UtcNow,
            EmailConfirmed = false,
            Status = UserStatus.Active
        };

        try
        {
            var result = await userManager.CreateAsync(user, request.Password!);
            if (result.Succeeded)
            {
                var response = new RegisteredUserResponse(user.Id, user.UserName!, user.CreatedAtUtc, user.EmailConfirmed, user.Status);
                return Results.Json(response, statusCode: StatusCodes.Status201Created);
            }

            if (result.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                return Results.Conflict(new ApiErrorResponse("account_conflict", "The email address or username is already in use."));
            }

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["password"] = result.Errors.Select(error => error.Description).ToArray()
            });
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return Results.Conflict(new ApiErrorResponse("account_conflict", "The email address or username is already in use."));
        }
    }

    private static Dictionary<string, string[]> Validate(RegisterRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email.Trim()))
        {
            errors["email"] = ["A valid email address is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Username) || !UsernamePattern().IsMatch(request.Username.Trim()))
        {
            errors["username"] = ["Username must contain 3 to 20 letters, numbers, or underscores."];
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            errors["password"] = ["A password is required."];
        }

        return errors;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    [GeneratedRegex("^[A-Za-z0-9_]{3,20}$")]
    private static partial Regex UsernamePattern();
}

public sealed record RegisterRequest(string? Email, string? Username, string? Password);

public sealed record RegisteredUserResponse(
    Guid Id,
    string Username,
    DateTime CreatedAtUtc,
    bool EmailConfirmed,
    UserStatus Status);

public sealed record ApiErrorResponse(string Code, string Message);
