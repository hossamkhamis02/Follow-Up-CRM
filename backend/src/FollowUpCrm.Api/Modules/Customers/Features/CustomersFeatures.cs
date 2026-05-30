using FluentValidation;
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

public sealed record CustomerListRequest(int Page = 1, int PageSize = 10, string? Search = null, string? SortBy = "name", string? SortDirection = "asc");

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(request => request.Phone)
            .MaximumLength(50);

        RuleFor(request => request.Company)
            .MaximumLength(200);
    }
}

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty();

        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(request => request.Phone)
            .MaximumLength(50);

        RuleFor(request => request.Company)
            .MaximumLength(200);
    }
}
