using Asp.Versioning;

namespace FollowUpCrm.Api.Modules.Dashboard;

public static class DashboardModule
{
    public static IServiceCollection AddDashboardModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapDashboardModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v{version:apiVersion}/dashboard")
            .WithTags("Dashboard")
            .HasApiVersion(1);

        return endpoints;
    }
}