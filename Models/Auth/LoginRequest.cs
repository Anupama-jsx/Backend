using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Auth;

public sealed record LoginRequest(
    [param: Required, EmailAddress] string? Email,
    [param: Required] string? Password);
