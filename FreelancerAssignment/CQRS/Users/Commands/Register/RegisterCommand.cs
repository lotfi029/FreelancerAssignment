using FreelancerAssignment.DTOs.Users;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace FreelancerAssignment.CQRS.Users.Commands.Register;

public sealed record RegisterCommand(string Username, string Password, string Email) : ICommand<AuthResponse>;

public sealed class RegisterCommandHandler(
    IUnitOfWork unitOfWork, 
    ILogger<RegisterCommandHandler> logger,
    IJwtProvider jwtProvider) : ICommandHandler<RegisterCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await unitOfWork.Users.ExistsAsync(e => e.Username == request.Username, cancellationToken))
                return UserErrors.UsernameNotUnique;

            if (await unitOfWork.Users.ExistsAsync(e => e.Email == request.Email, cancellationToken))
                return UserErrors.EmailNotUnique;

            var passwordHasher = new PasswordHasher<User>();
            var hashedPassword = passwordHasher.HashPassword(null!, request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            var token = jwtProvider.GenerateToken(user);

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(14);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration,
                CreateOn = DateTime.UtcNow
            });

            await unitOfWork.Users.AddAsync(user, cancellationToken);
            await unitOfWork.CommitChangesAsync(cancellationToken);

            var response = new AuthResponse(
                user.Id,
                user.Email,
                user.Username,
                token.token,
                token.expiresIn,
                refreshToken,
                refreshTokenExpiration
                );

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, message: "Error registering user");
            return Result.Failure<AuthResponse>(Error.FromException("RegisterUserError", ex.Message));
        }
    }
}