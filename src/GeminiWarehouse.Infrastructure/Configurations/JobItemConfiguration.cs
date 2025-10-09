using GeminiWarehouse.Domain.JobAggregate.Entities;
using GeminiWarehouse.Domain.JobAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeminiWarehouse.Infrastructure.Configurations;

public sealed class JobItemConfiguration : IEntityTypeConfiguration<JobItem>
{
    public void Configure(EntityTypeBuilder<JobItem> builder)
    {
        builder.ToTable("JobItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(
                id => id.Value,
                value => JobItemId.Create(value)
            );

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Quantity)
            .IsRequired();
    }
}