using Backend.Models.Auth;

namespace Backend.Services;

public enum LoginResultType
{
    Success,
    Invalid,
    Unauthorized
}

public sealed record LoginResult(
    LoginResultType Type,
    LoginResponse? Response = null,
    Dictionary<string, string[]>? ValidationErrors = null,
    ApiErrorResponse? Error = null);
