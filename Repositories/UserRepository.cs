using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        dbContext.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<bool> UsernameExistsAsync(string normalizedUsername, CancellationToken cancellationToken = default) =>
        dbContext.Users.AnyAsync(user => user.NormalizedUserName == normalizedUsername, cancellationToken);
}
