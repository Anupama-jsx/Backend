using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Backend.Identity;
using Backend.Models.Auth;
using Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public sealed partial class RegistrationService
{
    [GeneratedRegex("^[A-Za-z0-9_]{3,20}$")]
    private static partial Regex UsernamePattern();
}

public sealed partial class RegistrationService(
    IUserRepository userRepository,
    UserManager<ApplicationUser> userManager) : IRegistrationService
{
    public async Task<RegistrationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count != 0)
        {
            return new RegistrationResult(RegistrationResultType.Invalid, ValidationErrors: validationErrors);
        }

        var email = request.Email!.Trim();
        var username = request.Username!.Trim();
        var normalizedEmail = userManager.NormalizeEmail(email)!;
        var normalizedUsername = userManager.NormalizeName(username)!;

        // These checks provide friendly errors. The unique SQL Server indexes remain
        // the final authority when two requests arrive concurrently.
        if (await userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return Conflict("email_in_use", "An account already uses this email address.");
        }

        if (await userRepository.UsernameExistsAsync(normalizedUsername, cancellationToken))
        {
            return Conflict("username_in_use", "This username is unavailable.");
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
            // Identity validates the password and creates its one-way password hash.
            var result = await userManager.CreateAsync(user, request.Password!);
            if (result.Succeeded)
            {
                var response = new RegisteredUserResponse(user.Id, user.UserName!, user.CreatedAtUtc, user.EmailConfirmed, user.Status);
                return new RegistrationResult(RegistrationResultType.Created, User: response);
            }

            if (result.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                return Conflict("account_conflict", "The email address or username is already in use.");
            }

            return new RegistrationResult(
                RegistrationResultType.Invalid,
                ValidationErrors: new Dictionary<string, string[]>
                {
                    ["password"] = result.Errors.Select(error => error.Description).ToArray()
                });
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return Conflict("account_conflict", "The email address or username is already in use.");
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

    private static RegistrationResult Conflict(string code, string message) =>
        new(RegistrationResultType.Conflict, Error: new ApiErrorResponse(code, message));
}
