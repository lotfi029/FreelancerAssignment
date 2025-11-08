namespace FreelancerAssignment.DTOs.Users;

public sealed record LoginRequest(
    string UsernameOrEmail,
    string Password
    );
