namespace FreelancerAssignment.Abstractions;

public class ImageSettings
{
    public static readonly string ImagePath = @"uploads\images";

    public static readonly string[] AllowedExtension = new[] { ".jpg", ".jpeg", ".png" };
    
    public const int MaxFileSizeInBytes = 5 * 1024 * 1024;
}

