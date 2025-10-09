using GeminiWarehouse.Application.Common.Messaging;
using GeminiWarehouse.Domain.JobAggregate;
using GeminiWarehouse.Domain.JobAggregate.ValueObjects;

namespace GeminiWarehouse.Application.Common.Interfaces;

public interface IJobRepository : IRepository
{
    Task<Job?> GetByIdForUpdateAsync(JobId id, CancellationToken cancellationToken = default);
    Task<Job?> GetJobByFulfillmentIdAsync(Guid fulfillmentId, CancellationToken cancellationToken = default);
    Task<Job?> GetJobByOrderIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task AddAsync(Job job, CancellationToken cancellationToken = default);
}