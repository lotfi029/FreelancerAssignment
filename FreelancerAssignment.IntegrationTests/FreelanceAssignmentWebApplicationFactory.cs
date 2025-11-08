using FreelancerAssignment.Entities;
using FreelancerAssignment.IntegrationTests.Consts;
using FreelancerAssignment.Presistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FreelancerAssignment.IntegrationTests;
public class FreelanceAssignmentWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            var connectionString = GetConnectionString();
            services.AddSqlServer<ApplicationDbContext>(connectionString);

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = TestAuthHandler.TestScheme;
                options.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
                options.DefaultScheme = TestAuthHandler.TestScheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.TestScheme, options => { });

            var dbContext = CreateDbContext(services);
            dbContext.Database.EnsureDeleted();

            if (dbContext.Database.GetMigrations().Any())
                dbContext.Database.Migrate();

            SeedTestUser(dbContext);
            
        });
    }

    private static string? GetConnectionString ()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Testing.json")
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        return connectionString;
        
    }

    private static ApplicationDbContext CreateDbContext(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return dbContext;
    }

    private static void SeedTestUser(ApplicationDbContext dbContext)
    {

        if (dbContext.Users.Any())
            return;
        
        var testUser = new User
        {
            Id = TestUser.Id,
            Username = TestUser.UserName,
            Email = TestUser.Email,
            PasswordHash = TestUser.Password(),
            CreatedAt = DateTime.UtcNow,
            LastLoginTime = DateTime.Now
        };

        dbContext.Users.Add(testUser);
        dbContext.SaveChanges();
    }

}
