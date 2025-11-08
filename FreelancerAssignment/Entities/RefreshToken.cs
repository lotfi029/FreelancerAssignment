using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreelancerAssignment.Entities;

[Owned]
[Table("RefreshTokens")]
public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreateOn { get; set; }
    public DateTime? RevokeOn { get; set; }

    public bool IsExpired => ExpiresOn <= DateTime.UtcNow;
    public bool IsActive => RevokeOn is null && !IsExpired;

}
