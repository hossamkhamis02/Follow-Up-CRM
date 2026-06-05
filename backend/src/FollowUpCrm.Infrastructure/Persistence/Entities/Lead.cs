namespace FollowUpCrm.Infrastructure.Persistence.Entities;

public sealed class Lead : AuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public LeadSource Source { get; set; } = LeadSource.Other;

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public string? Notes { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public User? AssignedToUser { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }
}
