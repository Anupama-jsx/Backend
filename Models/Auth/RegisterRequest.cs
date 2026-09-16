using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Auth;

public sealed record RegisterRequest(
    [param: Required, EmailAddress] string? Email,
    [param: Required, StringLength(20, MinimumLength = 3), RegularExpression("^[A-Za-z0-9_]+$")] string? Username,
    [param: Required] string? Password);
