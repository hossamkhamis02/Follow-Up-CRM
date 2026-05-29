using Asp.Versioning;

namespace FollowUpCrm.Api.Modules.Customers;

public static class CustomersModule
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapCustomersModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v{version:apiVersion}/customers")
            .WithTags("Customers")
            .HasApiVersion(1);

        return endpoints;
    }
}