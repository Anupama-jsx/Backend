using Backend.Models.Auth;

namespace Backend.Services;

public enum RegistrationResultType
{
    Created,
    Invalid,
    Conflict
}

public sealed record RegistrationResult(
    RegistrationResultType Type,
    RegisteredUserResponse? User = null,
    Dictionary<string, string[]>? ValidationErrors = null,
    ApiErrorResponse? Error = null);
