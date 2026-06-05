using FollowUpCrm.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FollowUpCrm.Infrastructure.Persistence.Configurations;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");

        builder.HasKey(lead => lead.Id);

        builder.Property(lead => lead.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(lead => lead.CompanyName)
            .HasMaxLength(200);

        builder.Property(lead => lead.Email)
            .HasMaxLength(320);

        builder.Property(lead => lead.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(lead => lead.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(lead => lead.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(lead => lead.Notes)
            .HasMaxLength(2000);

        builder.Property(lead => lead.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(lead => lead.AssignedToUser)
            .WithMany()
            .HasForeignKey(lead => lead.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(lead => lead.Email);
        builder.HasIndex(lead => lead.PhoneNumber);
        builder.HasIndex(lead => lead.Status);
        builder.HasIndex(lead => lead.Source);
        builder.HasIndex(lead => lead.AssignedToUserId);
    }
}
