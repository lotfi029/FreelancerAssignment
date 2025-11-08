using FreelancerAssignment.Entities;

namespace FreelancerAssignment.IRepositories;

public interface IUserRepository : IRepository<User>
{

    Task<User?> GetUserByEmailOrUsername(string emailOrUsername, CancellationToken ct = default);
}

