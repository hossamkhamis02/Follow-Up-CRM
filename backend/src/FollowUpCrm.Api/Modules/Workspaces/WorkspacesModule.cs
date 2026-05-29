using Asp.Versioning;

namespace FollowUpCrm.Api.Modules.Workspaces;

public static class WorkspacesModule
{
    public static IServiceCollection AddWorkspacesModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapWorkspacesModule(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v{version:apiVersion}/workspaces")
            .WithTags("Workspaces")
            .HasApiVersion(1);

        return endpoints;
    }
}