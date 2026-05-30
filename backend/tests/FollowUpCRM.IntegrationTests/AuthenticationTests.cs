using System.Net.Http.Json;
using FluentAssertions;
using FollowUpCRM.IntegrationTests.Infrastructure;

namespace FollowUpCRM.IntegrationTests;

public sealed class AuthenticationTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwt()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = IntegrationTestFactory.AdminEmail,
            Password = IntegrationTestFactory.AdminPassword
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = IntegrationTestFactory.AdminEmail,
            Password = "WrongPassword123!"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
