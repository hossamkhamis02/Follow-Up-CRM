using FluentValidation;
using FollowUpCrm.Infrastructure.Persistence.Entities;

namespace FollowUpCrm.Api.Modules.Leads.Features;

public sealed record CreateLeadRequest(
    string FullName,
    string? CompanyName,
    string? Email,
    string? PhoneNumber,
    LeadSource Source,
    LeadStatus Status,
    string? Notes,
    Guid? AssignedToUserId);

public sealed record UpdateLeadRequest(
    string FullName,
    string? CompanyName,
    string? Email,
    string? PhoneNumber,
    LeadSource Source,
    LeadStatus Status,
    string? Notes,
    Guid? AssignedToUserId);

public sealed record LeadDto(
    Guid Id,
    string FullName,
    string? CompanyName,
    string? Email,
    string? PhoneNumber,
    LeadSource Source,
    LeadStatus Status,
    string? Notes,
    Guid? AssignedToUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy,
    bool IsDeleted);

public sealed record LeadListItemDto(
    Guid Id,
    string FullName,
    string? CompanyName,
    string? Email,
    string? PhoneNumber,
    LeadSource Source,
    LeadStatus Status,
    Guid? AssignedToUserId,
    DateTime CreatedAt);

public sealed record LeadQueryParameters(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    LeadStatus? Status = null,
    LeadSource? Source = null,
    Guid? AssignedToUserId = null,
    string? SortBy = "createdAt",
    string? SortDirection = "desc");

public sealed class CreateLeadRequestValidator : AbstractValidator<CreateLeadRequest>
{
    public CreateLeadRequestValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.CompanyName)
            .MaximumLength(200);

        RuleFor(request => request.Email)
            .EmailAddress()
            .MaximumLength(320)
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.PhoneNumber)
            .MaximumLength(50);

        RuleFor(request => request.Source)
            .IsInEnum();

        RuleFor(request => request.Status)
            .IsInEnum();

        RuleFor(request => request.Notes)
            .MaximumLength(2000);
    }
}

public sealed class UpdateLeadRequestValidator : AbstractValidator<UpdateLeadRequest>
{
    public UpdateLeadRequestValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.CompanyName)
            .MaximumLength(200);

        RuleFor(request => request.Email)
            .EmailAddress()
            .MaximumLength(320)
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.PhoneNumber)
            .MaximumLength(50);

        RuleFor(request => request.Source)
            .IsInEnum();

        RuleFor(request => request.Status)
            .IsInEnum();

        RuleFor(request => request.Notes)
            .MaximumLength(2000);
    }
}
