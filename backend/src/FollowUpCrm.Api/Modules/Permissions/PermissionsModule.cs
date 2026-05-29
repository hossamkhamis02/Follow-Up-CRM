using Asp.Versioning;
using FollowUpCrm.Api.Authentication;

namespace FollowUpCrm.Api.Modules.Permissions;

public static class PermissionsModule
{
    public static IServiceCollection AddPermissionsModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapPermissionsModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v{version:apiVersion}/permissions")
            .WithTags("Permissions")
            .HasApiVersion(1)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        return endpoints;
    }
}
