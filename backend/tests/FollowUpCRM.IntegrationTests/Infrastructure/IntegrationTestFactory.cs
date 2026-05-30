using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using FollowUpCrm.Api.Authentication;
using FollowUpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace FollowUpCRM.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@crm.local";
    public const string AdminPassword = "Admin123!";
    private const string JwtIssuer = "FollowUpCrm.IntegrationTests";
    private const string JwtAudience = "FollowUpCrm.IntegrationTests";
    private const string JwtSecret = "integration-test-secret-key-with-at-least-32-characters";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("followup_crm_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Secret"] = JwtSecret,
                ["Jwt:ExpiresInMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtIssuer,
                    ValidAudience = JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
                    NameClaimType = "FullName",
                    RoleClaimType = "Role",
                    ClockSkew = TimeSpan.Zero
                };
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Customers.IgnoreQueryFilters().ExecuteDeleteAsync();
        await dbContext.Users
            .IgnoreQueryFilters()
            .Where(user => user.Email != AdminEmail)
            .ExecuteDeleteAsync();
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = AdminEmail,
            Password = AdminPassword
        });

        response.EnsureSuccessStatusCode();

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        var token = json.RootElement.GetProperty("data").GetProperty("token").GetString();
        token.Should().NotBeNullOrWhiteSpace();
        new JwtSecurityTokenHandler().ReadJwtToken(token).Issuer.Should().Be(JwtIssuer);

        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}").Should().BeTrue();

        using var currentUserRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        currentUserRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}").Should().BeTrue();
        var currentUserResponse = await client.SendAsync(currentUserRequest);
        currentUserResponse.StatusCode.Should().Be(HttpStatusCode.OK, await currentUserResponse.Content.ReadAsStringAsync());

        return client;
    }
}
