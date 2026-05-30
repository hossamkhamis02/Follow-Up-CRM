using System.Net.Http.Json;
using FluentAssertions;
using FollowUpCRM.IntegrationTests.Infrastructure;

namespace FollowUpCRM.IntegrationTests;

public sealed class CustomersTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task CreateCustomer_WithValidRequest_CreatesCustomer()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/customers", new
        {
            Name = "Acme Contact",
            Email = "acme.contact@example.com",
            Phone = "+1-555-0100",
            Company = "Acme"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        json.RootElement.GetProperty("data").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCustomer_WithValidationFailure_ReturnsBadRequest()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/customers", new
        {
            Name = "",
            Email = "not-an-email",
            Phone = "",
            Company = ""
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomerById_WhenCustomerExists_ReturnsCustomer()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();
        var customerId = await CustomerTestData.CreateCustomerAsync(client, "Northwind Contact");

        // Act
        var response = await client.GetAsync($"/api/v1/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        json.RootElement.GetProperty("data").GetProperty("id").GetGuid().Should().Be(customerId);
        json.RootElement.GetProperty("data").GetProperty("name").GetString().Should().Be("Northwind Contact");
    }

    [Fact]
    public async Task UpdateCustomer_WithValidRequest_UpdatesCustomer()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();
        var customerId = await CustomerTestData.CreateCustomerAsync(client, "Original Contact");

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/customers/{customerId}", new
        {
            Id = customerId,
            Name = "Updated Contact",
            Email = "updated.contact@example.com",
            Phone = "+1-555-0200",
            Company = "Updated Co"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await client.GetAsync($"/api/v1/customers/{customerId}");
        using var json = await JsonDocumentHelpers.ReadAsync(getResponse);
        json.RootElement.GetProperty("data").GetProperty("name").GetString().Should().Be("Updated Contact");
        json.RootElement.GetProperty("data").GetProperty("email").GetString().Should().Be("updated.contact@example.com");
    }

    [Fact]
    public async Task DeleteCustomer_WhenCustomerExists_SoftDeletesCustomer()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();
        var customerId = await CustomerTestData.CreateCustomerAsync(client, "Delete Me");

        // Act
        var response = await client.DeleteAsync($"/api/v1/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await client.GetAsync($"/api/v1/customers/{customerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCustomers_WhenCustomerIsDeleted_DoesNotIncludeDeletedCustomer()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();
        var deletedCustomerId = await CustomerTestData.CreateCustomerAsync(client, "Hidden Contact");
        await CustomerTestData.CreateCustomerAsync(client, "Visible Contact");
        await client.DeleteAsync($"/api/v1/customers/{deletedCustomerId}");

        // Act
        var response = await client.GetAsync("/api/v1/customers?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        var names = json.RootElement.GetProperty("data").GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("name").GetString())
            .ToList();

        names.Should().Contain("Visible Contact");
        names.Should().NotContain("Hidden Contact");
    }
}
