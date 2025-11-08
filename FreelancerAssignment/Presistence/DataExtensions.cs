using Microsoft.EntityFrameworkCore;

namespace FreelancerAssignment.Presistence;

public static class DataExtensions
{
    public static async Task MigrateDbAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider
                                          .GetRequiredService <ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
