using Azure.Core;
using FreelancerAssignment.Entities;
using Microsoft.AspNetCore.Identity;

namespace FreelancerAssignment.IntegrationTests.Consts;
public class TestUser
{
    public static readonly Guid Id = Guid.Parse("0199D700-632B-76D2-8ED6-19B5F1F342B0");
    public const string Email = "Test@user.com";
    public static string Password()
    {
        var passwordHasher = new PasswordHasher<User>();
        var hashedPassword = passwordHasher.HashPassword(null!, "TestPassword");

        return hashedPassword;
    }
    public const string UserName = "Test_User";

}
