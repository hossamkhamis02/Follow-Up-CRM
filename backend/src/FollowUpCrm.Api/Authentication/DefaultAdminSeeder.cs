using FollowUpCrm.Infrastructure.Persistence;
using FollowUpCrm.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FollowUpCrm.Api.Authentication;

public static class DefaultAdminSeeder
{
    private const string DefaultAdminEmail = "admin@crm.local";
    private const string DefaultAdminPassword = "Admin123!";

    public static async Task SeedDefaultAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await dbContext.Users.IgnoreQueryFilters().AnyAsync(user => user.Email == DefaultAdminEmail))
            return;

        dbContext.Users.Add(new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = "Default Admin",
            Email = DefaultAdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultAdminPassword),
            Role = UserRole.Admin
        });

        await dbContext.SaveChangesAsync();
    }
}
