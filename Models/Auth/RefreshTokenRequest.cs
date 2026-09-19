using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Auth;

public sealed record RefreshTokenRequest(
    [property: Required] string? RefreshToken);
