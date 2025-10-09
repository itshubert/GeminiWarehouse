using GeminiWarehouse.Domain.Common.Models;
using GeminiWarehouse.Domain.JobAggregate.ValueObjects;

namespace GeminiWarehouse.Domain.JobAggregate.Entities;

public sealed class JobItem : Entity<JobItemId>
{
    public JobId JobId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }

    private JobItem(
        JobItemId id,
        JobId pickingJobId,
        Guid productId,
        string productName,
        int quantity) : base(id)
    {
        JobId = pickingJobId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
    }

#pragma warning disable CS8618
    private JobItem() : base(null!) { }
#pragma warning restore CS8618

    public static JobItem Create(
        Guid? jobItemId,
        JobId pickingJobId,
        Guid productId,
        string productName,
        int quantity)
    {
        return new JobItem(
            jobItemId is null ? JobItemId.CreateUnique() : JobItemId.Create(jobItemId.Value),
            pickingJobId,
            productId,
            productName,
            quantity);
    }
}