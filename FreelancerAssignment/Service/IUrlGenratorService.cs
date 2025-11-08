namespace FreelancerAssignment.Service;

public interface IUrlGenratorService
{
    string? GetImageUrl(Guid productId);
}
