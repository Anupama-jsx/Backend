using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Auth;

public sealed record RegisterRequest(
    [property: Required, EmailAddress] string? Email,
    [property: Required, StringLength(20, MinimumLength = 3), RegularExpression("^[A-Za-z0-9_]+$")] string? Username,
    [property: Required] string? Password);
