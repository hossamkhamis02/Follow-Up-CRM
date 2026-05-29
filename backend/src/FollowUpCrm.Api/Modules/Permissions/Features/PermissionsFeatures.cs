using MediatR;
using FollowUpCrm.Shared.Results;

namespace FollowUpCrm.Api.Modules.Permissions.Features;

public sealed record GetPermissionsQuery(Guid WorkspaceId) : IRequest<Result<List<PermissionResponse>>>;
public sealed record AssignPermissionCommand(Guid UserId, Guid WorkspaceId, string Role) : IRequest<Result<Guid>>;
public sealed record RemovePermissionCommand(Guid UserId, Guid WorkspaceId) : IRequest<Result<Guid>>;

public record PermissionResponse(Guid Id, string Role, Guid UserId, Guid WorkspaceId);