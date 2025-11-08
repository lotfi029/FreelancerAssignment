using FreelancerAssignment.DTOs.Users;
using System.Security.Cryptography;

namespace FreelancerAssignment.CQRS.Users.Commands.RefreshTokens;

public sealed record RefreshTokenCommand(string Token, string RefreshToken) : ICommand<AuthResponse>;

public sealed class RefreshTokenCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenCommandHandler> logger,
    IJwtProvider jwtProvider) : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = jwtProvider.ValidateToken(request.Token);
            if ( userId == Guid.Empty)
                return UserErrors.InvalidToken;

            if (await unitOfWork.Users.FindAsync(cancellationToken, userId) is not { } user)
                return UserErrors.InvalidUserId;

            var userRefreshToken = user.RefreshTokens
                .SingleOrDefault(e => e.Token == request.RefreshToken && e.IsActive);

            if (userRefreshToken is null)
                return UserErrors.NoRefreshToken;

            userRefreshToken.RevokeOn = DateTime.UtcNow;
            userRefreshToken.ExpiresOn = DateTime.UtcNow;

            var (newToken, expiresIn) = jwtProvider.GenerateToken(user);
            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(14);

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresOn = refreshTokenExpiration,
                CreateOn = DateTime.UtcNow
            });
            await unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await unitOfWork.CommitChangesAsync(cancellationToken);

            var response = new AuthResponse(
                user.Id,
                user.Email,
                user.Username,
                newToken,
                expiresIn,
                newRefreshToken,
                refreshTokenExpiration
                );
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return Result.Failure<AuthResponse>(Error.FromException("RefreshTokenError", ex.Message));
        }
    }
}