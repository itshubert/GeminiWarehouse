using GeminiWarehouse.Domain.JobAggregate;
using GeminiWarehouse.Domain.JobAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeminiWarehouse.Infrastructure.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => JobId.Create(value)
            );

        builder.Property(x => x.FulfillmentId)
            .IsRequired();

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_PickingJob_Status",
            "\"Status\" IN ('Pending', 'InProgress', 'Completed', 'Cancelled')"));

        builder.OwnsOne(o => o.ShippingAddress, sa =>
        {
            sa.Property(a => a.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("FirstName");
            sa.Property(a => a.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("LastName");
            sa.Property(a => a.AddressLine1)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("AddressLine1");

            sa.Property(a => a.AddressLine2)
                .HasMaxLength(200)
                .HasColumnName("AddressLine2");

            sa.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("City");

            sa.Property(a => a.State)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("State");

            sa.Property(a => a.PostCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("PostCode");

            sa.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("Country");
        });

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasMany(x => x.JobItems)
            .WithOne()
            .HasForeignKey(ji => ji.JobId)
            .HasPrincipalKey(j => j.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.JobItems)
            .HasField("_jobItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}