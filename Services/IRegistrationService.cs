using Backend.Models.Auth;

namespace Backend.Services;

public interface IRegistrationService
{
    Task<RegistrationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
