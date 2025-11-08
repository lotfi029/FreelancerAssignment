namespace FreelancerAssignment.CQRS.Users.Commands.RefreshTokens;

public sealed record RevokeRefreshTokenCommand(string Token, string RefreshToken) : ICommand;

public sealed class RevokeRefreshTokenCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenCommandHandler> logger,
    IJwtProvider jwtProvider) : ICommandHandler<RevokeRefreshTokenCommand>
{
    public async Task<Result> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = jwtProvider.ValidateToken(request.Token);
            if (userId == Guid.Empty)
                return UserErrors.InvalidToken;

            if (await unitOfWork.Users.FindAsync(cancellationToken, userId) is not { } user)
                return UserErrors.InvalidUserId;

            var userRefreshToken = user.RefreshTokens.SingleOrDefault(e => e.Token == request.RefreshToken && e.IsActive);

            if (userRefreshToken is null)
                return UserErrors.NoRefreshToken;

            userRefreshToken.RevokeOn = DateTime.UtcNow;
            userRefreshToken.ExpiresOn = DateTime.UtcNow;

            await unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await unitOfWork.CommitChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking refresh token");
            return Result.Failure(Error.FromException("RevokeRefreshTokenError", ex.Message));
        }
    }
}