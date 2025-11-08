using FreelancerAssignment.Abstractions;
using FreelancerAssignment.Abstractions.Messaging;
using FreelancerAssignment.DTOs.Users;
using FreelancerAssignment.Errors;
using FreelancerAssignment.IRepositories;
using Mapster;
using System.Runtime.InteropServices.Marshalling;

namespace FreelancerAssignment.CQRS.Users.Queries.Profile;

public sealed record GetProfileQuery(Guid UerId) : IQuery<ProfileResponse>;


public sealed class GetProfileQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetProfileQueryHandler> logger) : IQueryHandler<GetProfileQuery, ProfileResponse>
{
    public async Task<Result<ProfileResponse>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (await unitOfWork.Users.FindAsync(cancellationToken, request.UerId) is not { } user)
                return UserErrors.UserNotFound;

            var response = user.Adapt<ProfileResponse>();

            return response;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error while retriving user's profle");
            return Error.FromException("Get Profile Exception", ex.Message);
        }
    }
}
