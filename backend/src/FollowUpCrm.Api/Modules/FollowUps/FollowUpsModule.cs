using Asp.Versioning;
using FollowUpCrm.Api.Authentication;

namespace FollowUpCrm.Api.Modules.FollowUps;

public static class FollowUpsModule
{
    public static IServiceCollection AddFollowUpsModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapFollowUpsModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v{version:apiVersion}/followups")
            .WithTags("FollowUps")
            .HasApiVersion(1)
            .RequireAuthorization(AuthorizationPolicies.CrmUser);

        return endpoints;
    }
}
