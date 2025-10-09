using GeminiWarehouse.Domain.JobAggregate;

namespace GeminiWarehouse.Application.Common.Interfaces;

public interface IJobRepository
{
    Task AddAsync(Job job, CancellationToken cancellationToken = default);
}