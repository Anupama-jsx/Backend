using Backend.Models.Auth;

namespace Backend.Services;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}
