using Backend.Identity;

namespace Backend.Models.Auth;

public sealed record RegisteredUserResponse(
    Guid Id,
    string Username,
    DateTime CreatedAtUtc,
    bool EmailConfirmed,
    UserStatus Status);
