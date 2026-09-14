using Backend.Identity;

namespace Backend.Repositories;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(string normalizedUsername, CancellationToken cancellationToken = default);
}
