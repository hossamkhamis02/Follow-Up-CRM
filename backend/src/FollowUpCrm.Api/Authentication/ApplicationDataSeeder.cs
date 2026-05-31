using FollowUpCrm.Infrastructure.Persistence;
using FollowUpCrm.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FollowUpCrm.Api.Authentication;

public static class ApplicationDataSeeder
{
    private const string DefaultAdminEmail = "admin@crm.local";
    private const string DefaultAdminPassword = "Admin123!";

    private static readonly UserSeed[] DefaultUsers =
    [
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Default Admin",
            DefaultAdminEmail,
            DefaultAdminPassword,
            UserRole.Admin),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Sample Sales Rep",
            "sales.rep@crm.local",
            "SalesRep123!",
            UserRole.SalesRep),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Sample User",
            "user@crm.local",
            "User123!",
            UserRole.User)
    ];

    private static readonly Customer[] SampleCustomers =
    [
        new()
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Apex Manufacturing",
            Email = "contact@apex-manufacturing.test",
            Phone = "+1 555 0100",
            Company = "Apex Manufacturing"
        },
        new()
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Name = "Northwind Logistics",
            Email = "hello@northwind-logistics.test",
            Phone = "+1 555 0101",
            Company = "Northwind Logistics"
        },
        new()
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Name = "Contoso Retail",
            Email = "sales@contoso-retail.test",
            Phone = "+1 555 0102",
            Company = "Contoso Retail"
        }
    ];

    public static async Task SeedApplicationDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (app.Environment.IsEnvironment("Testing"))
            await dbContext.Database.MigrateAsync();

        await SeedUsersAsync(dbContext);
        await SeedCustomersAsync(dbContext);

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(ApplicationDbContext dbContext)
    {
        var seededEmails = DefaultUsers
            .Select(user => user.Email)
            .ToArray();

        var existingEmails = await dbContext.Users
            .IgnoreQueryFilters()
            .Where(user => seededEmails.Contains(user.Email))
            .Select(user => user.Email)
            .ToListAsync();

        var existingEmailSet = existingEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in DefaultUsers)
        {
            if (existingEmailSet.Contains(seed.Email))
                continue;

            dbContext.Users.Add(new User
            {
                Id = seed.Id,
                FullName = seed.FullName,
                Email = seed.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seed.Password),
                Role = seed.Role
            });
        }
    }

    private static async Task SeedCustomersAsync(ApplicationDbContext dbContext)
    {
        var sampleCustomerEmails = SampleCustomers
            .Select(customer => customer.Email)
            .ToArray();

        var existingEmails = await dbContext.Customers
            .IgnoreQueryFilters()
            .Where(customer => sampleCustomerEmails.Contains(customer.Email))
            .Select(customer => customer.Email)
            .ToListAsync();

        var existingEmailSet = existingEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seed in SampleCustomers)
        {
            if (existingEmailSet.Contains(seed.Email))
                continue;

            dbContext.Customers.Add(new Customer
            {
                Id = seed.Id,
                Name = seed.Name,
                Email = seed.Email,
                Phone = seed.Phone,
                Company = seed.Company
            });
        }
    }

    private sealed record UserSeed(
        Guid Id,
        string FullName,
        string Email,
        string Password,
        UserRole Role);
}
