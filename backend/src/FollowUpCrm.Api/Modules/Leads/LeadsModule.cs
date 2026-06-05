using System.Security.Claims;
using Asp.Versioning;
using FluentValidation;
using FollowUpCrm.Api.Authentication;
using FollowUpCrm.Api.Modules.Leads.Features;
using FollowUpCrm.Infrastructure.Persistence;
using FollowUpCrm.Infrastructure.Persistence.Entities;
using FollowUpCrm.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace FollowUpCrm.Api.Modules.Leads;

public static class LeadsModule
{
    public static IServiceCollection AddLeadsModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapLeadsModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints
            .NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("/api/v{version:apiVersion}/leads")
            .WithTags("Leads")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1)
            .RequireAuthorization(AuthorizationPolicies.CrmUser);

        group.MapPost("", CreateLeadAsync)
            .WithName("CreateLead")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        group.MapGet("", GetLeadsAsync)
            .WithName("GetLeads")
            .Produces<ApiResponse<PagedResponse<LeadListItemDto>>>();

        group.MapGet("/{id:guid}", GetLeadByIdAsync)
            .WithName("GetLeadById")
            .Produces<ApiResponse<LeadDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateLeadAsync)
            .WithName("UpdateLead")
            .Produces<ApiResponse<Guid>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteLeadAsync)
            .WithName("DeleteLead")
            .Produces<ApiResponse<Guid>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        var unversionedGroup = endpoints.MapGroup("/api/leads")
            .WithTags("Leads")
            .RequireAuthorization(AuthorizationPolicies.CrmUser);

        unversionedGroup.MapPost("", CreateLeadAsync)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        unversionedGroup.MapGet("", GetLeadsAsync)
            .Produces<ApiResponse<PagedResponse<LeadListItemDto>>>();

        unversionedGroup.MapGet("/{id:guid}", GetLeadByIdAsync)
            .Produces<ApiResponse<LeadDto>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        unversionedGroup.MapPut("/{id:guid}", UpdateLeadAsync)
            .Produces<ApiResponse<Guid>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        unversionedGroup.MapDelete("/{id:guid}", DeleteLeadAsync)
            .Produces<ApiResponse<Guid>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateLeadAsync(
        CreateLeadRequest request,
        IValidator<CreateLeadRequest> validator,
        ApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ValidationFailure(validationResult.Errors.Select(error => error.ErrorMessage));

        if (request.AssignedToUserId.HasValue && !await UserExistsAsync(request.AssignedToUserId.Value, dbContext, cancellationToken))
            return Results.BadRequest(ApiResponse<object>.FailureResponse("Assigned user was not found."));

        var userId = GetCurrentUserId(principal);
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            CompanyName = NormalizeOptionalText(request.CompanyName),
            Email = NormalizeEmail(request.Email),
            PhoneNumber = NormalizeOptionalText(request.PhoneNumber),
            Source = request.Source,
            Status = request.Status,
            Notes = NormalizeOptionalText(request.Notes),
            AssignedToUserId = request.AssignedToUserId,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        dbContext.Leads.Add(lead);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/leads/{lead.Id}", ApiResponse<Guid>.SuccessResponse(lead.Id));
    }

    private static async Task<IResult> GetLeadByIdAsync(
        Guid id,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var lead = await dbContext.Leads
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => new LeadDto(
                entity.Id,
                entity.FullName,
                entity.CompanyName,
                entity.Email,
                entity.PhoneNumber,
                entity.Source,
                entity.Status,
                entity.Notes,
                entity.AssignedToUserId,
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc,
                entity.DeletedAtUtc,
                entity.CreatedBy,
                entity.UpdatedBy,
                entity.IsDeleted))
            .SingleOrDefaultAsync(cancellationToken);

        return lead is null
            ? Results.NotFound(ApiResponse<object>.FailureResponse("Lead was not found."))
            : Results.Ok(ApiResponse<LeadDto>.SuccessResponse(lead));
    }

    private static async Task<IResult> GetLeadsAsync(
        [AsParameters] LeadQueryParameters parameters,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(parameters.Page, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var query = dbContext.Leads.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(lead =>
                lead.FullName.ToLower().Contains(search)
                || (lead.CompanyName != null && lead.CompanyName.ToLower().Contains(search))
                || (lead.Email != null && lead.Email.ToLower().Contains(search))
                || (lead.PhoneNumber != null && lead.PhoneNumber.ToLower().Contains(search)));
        }

        if (parameters.Status.HasValue)
            query = query.Where(lead => lead.Status == parameters.Status.Value);

        if (parameters.Source.HasValue)
            query = query.Where(lead => lead.Source == parameters.Source.Value);

        if (parameters.AssignedToUserId.HasValue)
            query = query.Where(lead => lead.AssignedToUserId == parameters.AssignedToUserId.Value);

        query = ApplySorting(query, parameters.SortBy, parameters.SortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var leads = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(lead => new LeadListItemDto(
                lead.Id,
                lead.FullName,
                lead.CompanyName,
                lead.Email,
                lead.PhoneNumber,
                lead.Source,
                lead.Status,
                lead.AssignedToUserId,
                lead.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Results.Ok(ApiResponse<PagedResponse<LeadListItemDto>>.SuccessResponse(
            new PagedResponse<LeadListItemDto>(leads, totalCount, page, pageSize)));
    }

    private static async Task<IResult> UpdateLeadAsync(
        Guid id,
        UpdateLeadRequest request,
        IValidator<UpdateLeadRequest> validator,
        ApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ValidationFailure(validationResult.Errors.Select(error => error.ErrorMessage));

        if (request.AssignedToUserId.HasValue && !await UserExistsAsync(request.AssignedToUserId.Value, dbContext, cancellationToken))
            return Results.BadRequest(ApiResponse<object>.FailureResponse("Assigned user was not found."));

        var lead = await dbContext.Leads.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (lead is null)
            return Results.NotFound(ApiResponse<object>.FailureResponse("Lead was not found."));

        lead.FullName = request.FullName.Trim();
        lead.CompanyName = NormalizeOptionalText(request.CompanyName);
        lead.Email = NormalizeEmail(request.Email);
        lead.PhoneNumber = NormalizeOptionalText(request.PhoneNumber);
        lead.Source = request.Source;
        lead.Status = request.Status;
        lead.Notes = NormalizeOptionalText(request.Notes);
        lead.AssignedToUserId = request.AssignedToUserId;
        lead.UpdatedBy = GetCurrentUserId(principal);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<Guid>.SuccessResponse(lead.Id));
    }

    private static async Task<IResult> DeleteLeadAsync(
        Guid id,
        ApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var lead = await dbContext.Leads.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (lead is null)
            return Results.NotFound(ApiResponse<object>.FailureResponse("Lead was not found."));

        lead.UpdatedBy = GetCurrentUserId(principal);
        dbContext.Leads.Remove(lead);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<Guid>.SuccessResponse(id));
    }

    private static IQueryable<Lead> ApplySorting(
        IQueryable<Lead> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "name" or "fullname" => descending ? query.OrderByDescending(lead => lead.FullName) : query.OrderBy(lead => lead.FullName),
            "createdat" or "created" => descending ? query.OrderByDescending(lead => lead.CreatedAtUtc) : query.OrderBy(lead => lead.CreatedAtUtc),
            _ => descending ? query.OrderByDescending(lead => lead.CreatedAtUtc) : query.OrderBy(lead => lead.CreatedAtUtc)
        };
    }

    private static async Task<bool> UserExistsAsync(
        Guid userId,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken);

    private static Guid? GetCurrentUserId(ClaimsPrincipal principal)
        => Guid.TryParse(principal.FindFirstValue("UserId"), out var userId) ? userId : null;

    private static string? NormalizeOptionalText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeEmail(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static IResult ValidationFailure(IEnumerable<string> errors)
        => Results.BadRequest(ApiResponse<object>.FailureResponse("Validation failed.", errors));
}
