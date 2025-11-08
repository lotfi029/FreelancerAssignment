using System.ComponentModel.DataAnnotations;

namespace FreelancerAssignment.Authentication;

public class JwtOptions
{
    public static string SectionName = nameof(JwtOptions);
    [Required]
    public string Key { get; set; } = string.Empty;
    [Required]
    public string Issuer { get; set; } = string.Empty;
    [Required]
    public string Audience { get; set; } = string.Empty;
    [Required]
    [Range(1, int.MaxValue)]
    public int ExpiryMinutes { get; set; }
}

