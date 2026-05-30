using Asp.Versioning;
using FollowUpCrm.Api.Authentication;
using FollowUpCrm.Api.Modules.Customers.Features;
using FollowUpCrm.Infrastructure.Persistence;
using FollowUpCrm.Infrastructure.Persistence.Entities;
using FollowUpCrm.Shared.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FollowUpCrm.Api.Modules.Customers;

public static class CustomersModule
{
    public static IServiceCollection AddCustomersModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapCustomersModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints
            .NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("/api/v{version:apiVersion}/customers")
            .WithTags("Customers")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .RequireAuthorization(AuthorizationPolicies.SalesRepOrAdmin);

        group.MapPost("", CreateCustomerAsync)
            .WithName("CreateCustomer")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        group.MapGet("", GetCustomersAsync)
            .WithName("GetCustomers")
            .Produces<ApiResponse<PagedResponse<CustomerResponse>>>();

        group.MapGet("/{id:guid}", GetCustomerByIdAsync)
            .WithName("GetCustomerById")
            .Produces<ApiResponse<CustomerResponse>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateCustomerAsync)
            .WithName("UpdateCustomer")
            .Produces<ApiResponse<Guid>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteCustomerAsync)
            .WithName("DeleteCustomer")
            .Produces<ApiResponse<Guid>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateCustomerAsync(
        CreateCustomerCommand command,
        IValidator<CreateCustomerCommand> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return ValidationFailure(validationResult.Errors.Select(error => error.ErrorMessage));

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Email = command.Email.Trim().ToLowerInvariant(),
            Phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim(),
            Company = string.IsNullOrWhiteSpace(command.Company) ? null : command.Company.Trim()
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/customers/{customer.Id}", ApiResponse<Guid>.SuccessResponse(customer.Id));
    }

    private static async Task<IResult> GetCustomersAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        string? sortDirection,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(customer => customer.Name.ToLower().Contains(normalizedSearch));
        }

        query = ApplySorting(query, sortBy, sortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var customers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(customer => new CustomerResponse(
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.Company,
                customer.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<PagedResponse<CustomerResponse>>.SuccessResponse(
            new PagedResponse<CustomerResponse>(customers, totalCount, page, pageSize)));
    }

    private static async Task<IResult> GetCustomerByIdAsync(
        Guid id,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => new CustomerResponse(
                entity.Id,
                entity.Name,
                entity.Email,
                entity.Phone,
                entity.Company,
                entity.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return customer is null
            ? Results.NotFound(ApiResponse<object>.FailureResponse("Customer was not found."))
            : Results.Ok(ApiResponse<CustomerResponse>.SuccessResponse(customer));
    }

    private static async Task<IResult> UpdateCustomerAsync(
        Guid id,
        UpdateCustomerCommand request,
        IValidator<UpdateCustomerCommand> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return ValidationFailure(validationResult.Errors.Select(error => error.ErrorMessage));

        var customer = await dbContext.Customers.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (customer is null)
            return Results.NotFound(ApiResponse<object>.FailureResponse("Customer was not found."));

        customer.Name = command.Name.Trim();
        customer.Email = command.Email.Trim().ToLowerInvariant();
        customer.Phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim();
        customer.Company = string.IsNullOrWhiteSpace(command.Company) ? null : command.Company.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<Guid>.SuccessResponse(customer.Id));
    }

    private static async Task<IResult> DeleteCustomerAsync(
        Guid id,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (customer is null)
            return Results.NotFound(ApiResponse<object>.FailureResponse("Customer was not found."));

        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<Guid>.SuccessResponse(id));
    }

    private static IQueryable<Customer> ApplySorting(
        IQueryable<Customer> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "email" => descending ? query.OrderByDescending(customer => customer.Email) : query.OrderBy(customer => customer.Email),
            "company" => descending ? query.OrderByDescending(customer => customer.Company) : query.OrderBy(customer => customer.Company),
            "createdat" => descending ? query.OrderByDescending(customer => customer.CreatedAtUtc) : query.OrderBy(customer => customer.CreatedAtUtc),
            _ => descending ? query.OrderByDescending(customer => customer.Name) : query.OrderBy(customer => customer.Name)
        };
    }

    private static IResult ValidationFailure(IEnumerable<string> errors)
        => Results.BadRequest(ApiResponse<object>.FailureResponse("Validation failed.", errors));
}
