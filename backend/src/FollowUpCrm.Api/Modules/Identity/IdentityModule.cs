using Asp.Versioning;

namespace FollowUpCrm.Api.Modules.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v{version:apiVersion}/identity")
            .WithTags("Identity")
            .HasApiVersion(1);

        return endpoints;
    }
}