namespace FreelancerAssignment.DTOs.Users;

public record AuthResponse(
    Guid Id,
    string Email,
    string Username,
    string Token,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiration
);
