using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Data;
using Backend.Identity;
using Backend.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public sealed class LoginService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IConfiguration configuration) : ILoginService
{
    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var errors = Validate(request);
        if (errors.Count != 0)
        {
            return new LoginResult(LoginResultType.Invalid, ValidationErrors: errors);
        }

        var user = await userManager.FindByEmailAsync(request.Email!.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password!))
        {
            return Unauthorized();
        }

        if (user.Status != UserStatus.Active)
        {
            return Unauthorized();
        }

        return await CreateTokenResponseAsync(user, cancellationToken);
    }

    public async Task<LoginResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return new LoginResult(LoginResultType.Invalid, ValidationErrors: new Dictionary<string, string[]>
            {
                ["refreshToken"] = ["A refresh token is required."]
            });
        }

        var now = DateTime.UtcNow;
        var tokenHash = HashToken(request.RefreshToken);
        var refreshToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null || refreshToken.RevokedAtUtc is not null || refreshToken.ExpiresAtUtc <= now)
        {
            return InvalidRefreshToken();
        }

        var user = await userManager.FindByIdAsync(refreshToken.UserId.ToString());
        if (user is null || user.Status != UserStatus.Active)
        {
            return InvalidRefreshToken();
        }

        var replacement = CreateRefreshToken(user.Id, now);
        refreshToken.RevokedAtUtc = now;
        refreshToken.ReplacedByTokenId = replacement.Entity.Id;
        dbContext.RefreshTokens.Add(replacement.Entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            return InvalidRefreshToken();
        }

        return CreateTokenResponse(user, replacement.PlainTextToken, replacement.Entity.ExpiresAtUtc);
    }

    private async Task<LoginResult> CreateTokenResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var refreshToken = CreateRefreshToken(user.Id, now);
        dbContext.RefreshTokens.Add(refreshToken.Entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreateTokenResponse(user, refreshToken.PlainTextToken, refreshToken.Entity.ExpiresAtUtc);
    }

    private LoginResult CreateTokenResponse(
        ApplicationUser user,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc)
    {
        var issuedAtUtc = DateTime.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(GetExpiryMinutes());
        var accessToken = CreateAccessToken(user, issuedAtUtc, expiresAtUtc);

        return new LoginResult(
            LoginResultType.Success,
            new LoginResponse(
                accessToken,
                "Bearer",
                expiresAtUtc,
                refreshToken,
                refreshTokenExpiresAtUtc));
    }

    private (RefreshToken Entity, string PlainTextToken) CreateRefreshToken(Guid userId, DateTime now)
    {
        var plainTextToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (
            new RefreshToken
            {
                UserId = userId,
                TokenHash = HashToken(plainTextToken),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddDays(GetRefreshTokenExpiryDays())
            },
            plainTextToken);
    }

    private string CreateAccessToken(ApplicationUser user, DateTime issuedAtUtc, DateTime expiresAtUtc)
    {
        var issuer = configuration["Jwt:Issuer"]!;
        var audience = configuration["Jwt:Audience"]!;
        var key = configuration["Jwt:Key"]!;
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!)
        };

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer,
            audience,
            claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: credentials));
    }

    private int GetExpiryMinutes() =>
        int.TryParse(configuration["Jwt:ExpiryMinutes"], out var minutes) && minutes is > 0 and <= 1_440
            ? minutes
            : 60;

    private int GetRefreshTokenExpiryDays() =>
        int.TryParse(configuration["Jwt:RefreshTokenExpiryDays"], out var days) && days is > 0 and <= 365
            ? days
            : 30;

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static Dictionary<string, string[]> Validate(LoginRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email.Trim()))
        {
            errors["email"] = ["A valid email address is required."];
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            errors["password"] = ["A password is required."];
        }

        return errors;
    }

    private static LoginResult Unauthorized() =>
        new(LoginResultType.Unauthorized, Error: new ApiErrorResponse("invalid_credentials", "Email or password is incorrect."));

    private static LoginResult InvalidRefreshToken() =>
        new(LoginResultType.Unauthorized, Error: new ApiErrorResponse("invalid_refresh_token", "The refresh token is invalid or expired."));
}
