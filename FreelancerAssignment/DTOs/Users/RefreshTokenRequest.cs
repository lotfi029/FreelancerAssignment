namespace FreelancerAssignment.DTOs.Users;

public sealed record RefreshTokenRequest(
    string Token,
    string RefreshToken
    );
