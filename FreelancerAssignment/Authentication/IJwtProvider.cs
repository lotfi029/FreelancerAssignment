using FreelancerAssignment.Entities;

namespace FreelancerAssignment.Authentication;

public interface IJwtProvider
{
    (string token, int expiresIn) GenerateToken(User user);
    Guid ValidateToken(string token);
}
