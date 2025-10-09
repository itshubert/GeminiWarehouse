using GeminiWarehouse.Domain.Common.Models;
using GeminiWarehouse.Domain.JobAggregate.Entities;
using GeminiWarehouse.Domain.JobAggregate.ValueObjects;

namespace GeminiWarehouse.Domain.JobAggregate;

public sealed class Job : AggregateRoot<JobId>
{
    private readonly List<JobItem> _jobItems = new();
    public Guid FulfillmentId { get; private set; }
    public Guid OrderId { get; private set; }
    public JobStatus Status { get; private set; }
    public ShippingAddress ShippingAddress { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<JobItem> JobItems => _jobItems.AsReadOnly();

    private Job(
        JobId id,
        Guid fulfillmentId,
        Guid orderId,
        JobStatus status,
        ShippingAddress shippingAddress,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) : base(id)
    {
        FulfillmentId = fulfillmentId;
        OrderId = orderId;
        Status = status;
        ShippingAddress = shippingAddress;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

#pragma warning disable CS8618
    private Job() : base(null!) { }
#pragma warning restore CS8618

    public static Job Create(
        Guid? pickingJobId,
        Guid fulfillmentId,
        Guid orderId,
        JobStatus status,
        ShippingAddress shippingAddress,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        return new Job(
            pickingJobId is null ? JobId.CreateUnique() : JobId.Create(pickingJobId.Value),
            fulfillmentId,
            orderId,
            status,
            shippingAddress,
            createdAt ?? DateTimeOffset.UtcNow,
            updatedAt ?? DateTimeOffset.UtcNow);
    }

    public void AddJobItem(JobItem jobItem)
    {
        if (jobItem is null)
        {
            throw new ArgumentNullException(nameof(jobItem));
        }

        if (_jobItems.Any(ji => ji.Id == jobItem.Id))
        {
            throw new InvalidOperationException("Job item with the same ID already exists in the picking job.");
        }

        _jobItems.Add(jobItem);
    }

    public void RemoveJobItem(JobItem jobItem)
    {
        if (jobItem is null)
        {
            throw new ArgumentNullException(nameof(jobItem));
        }

        if (!_jobItems.Remove(jobItem))
        {
            throw new InvalidOperationException("Job item not found in the picking job.");
        }
    }

    public void UpdateStatus(JobStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

}
