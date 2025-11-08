namespace FreelancerAssignment.DTOs.Users;

public record UserResponse(Guid Id, string Username, string Email, DateTime CreatedAt, DateTime? LastLoginTime);