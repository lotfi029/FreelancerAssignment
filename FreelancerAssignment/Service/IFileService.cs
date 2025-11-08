using FreelancerAssignment.Abstractions;

namespace FreelancerAssignment.Service;

public interface IFileService
{
    Task<Result<string>> UploadImageAsync(IFormFile image, CancellationToken ct = default);
    Task<(FileStream? stream, string? contentType, string? fileName)> StreamAsync(string image, CancellationToken ct = default);
    Task<Result> DeleteImageAsync(string image);

}
