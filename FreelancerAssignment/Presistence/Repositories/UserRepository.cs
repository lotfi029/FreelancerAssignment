using FreelancerAssignment.Entities;
using FreelancerAssignment.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace FreelancerAssignment.Presistence.Repositories;

public class UserRepository(
    ApplicationDbContext context,
    ILogger<Repository<User>> repositoryLogger) : Repository<User>(context, repositoryLogger), IUserRepository
{
    public async Task<User?> GetUserByEmailOrUsername(string emailOrUsername, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(emailOrUsername))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(emailOrUsername));

        return await _context.Users
            .SingleOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername, ct);
    }
}
