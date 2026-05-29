using MediatR;
using FollowUpCrm.Shared.Results;

namespace FollowUpCrm.Api.Modules.FollowUps.Features;

public sealed record CreateFollowUpCommand(Guid CustomerId, string Type, DateTime ScheduledAt, string? Notes) : IRequest<Result<Guid>>;
public sealed record UpdateFollowUpCommand(Guid Id, string Type, DateTime ScheduledAt, string? Notes, string Status) : IRequest<Result<Guid>>;
public sealed record DeleteFollowUpCommand(Guid Id) : IRequest<Result<Guid>>;
public sealed record GetFollowUpByIdQuery(Guid Id) : IRequest<Result<FollowUpResponse>>;
public sealed record GetFollowUpsQuery(Guid CustomerId, int Page, int PageSize) : IRequest<Result<PagedResponse<FollowUpResponse>>>;

public record FollowUpResponse(Guid Id, Guid CustomerId, string Type, DateTime ScheduledAt, string? Notes, string Status, DateTime CreatedAt);