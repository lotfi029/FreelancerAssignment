using FreelancerAssignment.Abstractions;

namespace FreelancerAssignment.Service;

public class FileService(IWebHostEnvironment env, ILogger<FileService> logger) : IFileService
{
    private static readonly string _ImagePath = ImageSettings.ImagePath;
    private readonly string _path = Path.Combine(env.WebRootPath, _ImagePath);

    public Task<Result> DeleteImageAsync(string image)
    {
        if (string.IsNullOrEmpty(image))
            return Task.FromResult(Result.Success());

        try
        {
            var fileName = Path.GetFileName(image);
            var imagePath = Path.Combine(_path, fileName);

            if (File.Exists(imagePath))
                File.Delete(imagePath);

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting image {Image}", image);
            throw;
        }
    }

    public Task<(FileStream? stream, string? contentType, string? fileName)> StreamAsync(string image, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(image)) return Task.FromResult<(FileStream?, string?, string?)>(default);

        var fileName = Path.GetFileName(image);
        var imagePath = Path.Combine(_path, fileName);

        if (!File.Exists(imagePath)) return Task.FromResult<(FileStream?, string?, string?)>(default);

        var contentType = Path.GetExtension(imagePath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        var stream = File.OpenRead(imagePath);
        return Task.FromResult<(FileStream?, string?, string?)>((stream, contentType, fileName));
    }

    public async Task<Result<string>> UploadImageAsync(IFormFile image, CancellationToken ct = default)
    {
        try
        {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            var uniqueImageName = $"{Guid.CreateVersion7()}{Path.GetExtension(image.FileName)}";
            var imagePath = Path.Combine(_path, uniqueImageName);

            using var fileStream = new FileStream(imagePath, FileMode.Create);
            await image.CopyToAsync(fileStream, ct);

            return Result.Success(uniqueImageName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading image {ImageName}", image.FileName);
            throw; 
        }
    }
}
