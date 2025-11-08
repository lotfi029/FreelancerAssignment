namespace FreelancerAssignment.DTOs.Users;

public sealed record RegisterRequest(
    string Username, 
    string Email,
    string Password
    );
