namespace FollowUpCrm.Infrastructure.Persistence.Entities;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTime? DeletedAtUtc { get; set; }
}
