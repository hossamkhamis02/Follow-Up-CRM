using MediatR;
using FollowUpCrm.Shared.Results;

namespace FollowUpCrm.Api.Modules.Dashboard.Features;

public sealed record GetDashboardStatsQuery(Guid WorkspaceId) : IRequest<Result<DashboardStatsResponse>>;
public sealed record GetRecentFollowUpsQuery(Guid WorkspaceId, int Count) : IRequest<Result<List<RecentFollowUpResponse>>>;
public sealed record GetUpcomingFollowUpsQuery(Guid WorkspaceId, int DaysAhead) : IRequest<Result<List<UpcomingFollowUpResponse>>>;

public record DashboardStatsResponse(int TotalCustomers, int TotalFollowUps, int PendingFollowUps, int CompletedFollowUps);
public record RecentFollowUpResponse(Guid Id, string CustomerName, string Type, string Status, DateTime CreatedAt);
public record UpcomingFollowUpResponse(Guid Id, string CustomerName, string Type, DateTime ScheduledAt);