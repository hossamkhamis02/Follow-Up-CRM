using FluentAssertions;
using FollowUpCRM.IntegrationTests.Infrastructure;

namespace FollowUpCRM.IntegrationTests;

public sealed class PaginationAndFilteringTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task GetCustomers_WithPageSize_ReturnsRequestedNumberOfItems()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();
        await SeedCustomersAsync(client);

        // Act
        var response = await client.GetAsync("/api/v1/customers?page=1&pageSize=2&sortBy=name&sortDirection=asc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        json.RootElement.GetProperty("data").GetProperty("pageSize").GetInt32().Should().Be(2);
        json.RootElement.GetProperty("data").GetProperty("items").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GetCustomers_WithPageNumber_ReturnsRequestedPage()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();
        await SeedCustomersAsync(client);

        // Act
        var response = await client.GetAsync("/api/v1/customers?page=2&pageSize=2&sortBy=name&sortDirection=asc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        var data = json.RootElement.GetProperty("data");
        var names = data.GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("name").GetString())
            .ToList();

        data.GetProperty("page").GetInt32().Should().Be(2);
        names.Should().Equal("Charlie Contact", "Delta Contact");
    }

    [Fact]
    public async Task GetCustomers_WithSearchByName_ReturnsMatchingCustomers()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();
        await SeedCustomersAsync(client);

        // Act
        var response = await client.GetAsync("/api/v1/customers?page=1&pageSize=10&search=alp");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        var names = json.RootElement.GetProperty("data").GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("name").GetString())
            .ToList();

        names.Should().Equal("Alpha Contact");
    }

    [Fact]
    public async Task GetCustomers_WithSorting_ReturnsSortedCustomers()
    {
        // Arrange
        await factory.ResetDatabaseAsync();
        var client = await factory.CreateAuthenticatedClientAsync();
        await SeedCustomersAsync(client);

        // Act
        var response = await client.GetAsync("/api/v1/customers?page=1&pageSize=4&sortBy=name&sortDirection=desc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        var names = json.RootElement.GetProperty("data").GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("name").GetString())
            .ToList();

        names.Should().Equal("Delta Contact", "Charlie Contact", "Bravo Contact", "Alpha Contact");
    }

    private static async Task SeedCustomersAsync(HttpClient client)
    {
        await CustomerTestData.CreateCustomerAsync(client, "Alpha Contact");
        await CustomerTestData.CreateCustomerAsync(client, "Bravo Contact");
        await CustomerTestData.CreateCustomerAsync(client, "Charlie Contact");
        await CustomerTestData.CreateCustomerAsync(client, "Delta Contact");
    }
}
