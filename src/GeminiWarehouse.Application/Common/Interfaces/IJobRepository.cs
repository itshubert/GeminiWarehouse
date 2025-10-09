using GeminiWarehouse.Domain.JobAggregate;

namespace GeminiWarehouse.Application.Common.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetJobByFulfillmentIdAsync(Guid fulfillmentId, CancellationToken cancellationToken = default);
    Task<Job?> GetJobByOrderIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task AddAsync(Job job, CancellationToken cancellationToken = default);
}