using System.Net.Http.Json;
using FluentAssertions;

namespace FollowUpCRM.IntegrationTests.Infrastructure;

public static class CustomerTestData
{
    public static async Task<Guid> CreateCustomerAsync(
        HttpClient client,
        string name,
        string? email = null,
        string? phone = null,
        string? company = null)
    {
        var response = await client.PostAsJsonAsync("/api/v1/customers", new
        {
            Name = name,
            Email = email ?? $"{Slug(name)}@example.com",
            Phone = phone,
            Company = company
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var json = await JsonDocumentHelpers.ReadAsync(response);
        return json.RootElement.GetProperty("data").GetGuid();
    }

    private static string Slug(string value)
        => value.Trim().ToLowerInvariant().Replace(" ", ".");
}
