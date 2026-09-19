namespace Backend.Models.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
