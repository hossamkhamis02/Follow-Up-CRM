using System.Text.Json;

namespace FollowUpCRM.IntegrationTests.Infrastructure;

public static class JsonDocumentHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<JsonDocument> ReadAsync(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }
}
