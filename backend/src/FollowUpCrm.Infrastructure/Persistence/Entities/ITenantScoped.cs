namespace FollowUpCrm.Infrastructure.Persistence.Entities;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
