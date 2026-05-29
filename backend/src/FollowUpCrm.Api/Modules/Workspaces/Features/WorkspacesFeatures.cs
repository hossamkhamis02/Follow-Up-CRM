using MediatR;
using FollowUpCrm.Shared.Results;

namespace FollowUpCrm.Api.Modules.Workspaces.Features;

public sealed record CreateWorkspaceCommand(string Name, string Description) : IRequest<Result<Guid>>;
public sealed record UpdateWorkspaceCommand(Guid Id, string Name, string Description) : IRequest<Result<Guid>>;
public sealed record DeleteWorkspaceCommand(Guid Id) : IRequest<Result<Guid>>;
public sealed record GetWorkspaceByIdQuery(Guid Id) : IRequest<Result<WorkspaceResponse>>;
public sealed record GetWorkspacesQuery() : IRequest<Result<List<WorkspaceResponse>>>;

public record WorkspaceResponse(Guid Id, string Name, string Description, DateTime CreatedAt);