using FollowUpCrm.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FollowUpCrm.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(customer => customer.Phone)
            .HasMaxLength(50);

        builder.Property(customer => customer.Company)
            .HasMaxLength(200);

        builder.Property(customer => customer.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(customer => customer.Email)
            .IsUnique();
    }
}
