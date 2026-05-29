using MediatR;
using FollowUpCrm.Shared.Results;

namespace FollowUpCrm.Api.Modules.Customers.Features;

public sealed record CreateCustomerCommand(string Name, string Email, string? Phone, string? Company) : IRequest<Result<Guid>>;
public sealed record UpdateCustomerCommand(Guid Id, string Name, string Email, string? Phone, string? Company) : IRequest<Result<Guid>>;
public sealed record DeleteCustomerCommand(Guid Id) : IRequest<Result<Guid>>;
public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<Result<CustomerResponse>>;
public sealed record GetCustomersQuery(Guid WorkspaceId, int Page, int PageSize) : IRequest<Result<PagedResponse<CustomerResponse>>>;
public sealed record SearchCustomersQuery(Guid WorkspaceId, string SearchTerm, int Page, int PageSize) : IRequest<Result<PagedResponse<CustomerResponse>>>;

public record CustomerResponse(Guid Id, string Name, string Email, string? Phone, string? Company, DateTime CreatedAt);