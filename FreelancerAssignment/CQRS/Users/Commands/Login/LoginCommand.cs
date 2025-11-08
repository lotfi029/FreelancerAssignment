using FreelancerAssignment.Abstractions;
using FreelancerAssignment.Abstractions.Messaging;
using FreelancerAssignment.Authentication;
using FreelancerAssignment.DTOs.Users;
using FreelancerAssignment.Entities;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace FreelancerAssignment.CQRS.Users.Commands.Login;

public sealed record LoginCommand(string UsernameOrEmail, string Password) : ICommand<AuthResponse>;

public sealed class LoginCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<LoginCommandHandler> logger,
    IJwtProvider jwtProvider
    ) : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try 
        {
            if (await unitOfWork.Users.GetUserByEmailOrUsername(request.UsernameOrEmail, cancellationToken) is not { } user)
                return UserErrors.InvalidCredentials;

            var passwordHasher = new PasswordHasher<User>();

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                return UserErrors.InvalidCredentials;

            var token = jwtProvider.GenerateToken(user);

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(14);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration,
            });

            await unitOfWork.Users.UpdateAsync(user, cancellationToken);
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
            logger.LogError(ex, "Error logging in user");
            return Result.Failure<AuthResponse>(Error.FromException("LoginUserError", ex.Message));
        }
    }
        
}

