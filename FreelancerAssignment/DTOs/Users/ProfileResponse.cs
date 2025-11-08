namespace FreelancerAssignment.DTOs.Users;

public record ProfileResponse(
    Guid Id,
    string Email,
    string Username,
    DateTime LastLoginTime
    );