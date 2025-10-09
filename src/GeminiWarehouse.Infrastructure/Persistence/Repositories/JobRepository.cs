using GeminiWarehouse.Application.Common.Interfaces;
using GeminiWarehouse.Domain.JobAggregate;

namespace GeminiWarehouse.Infrastructure.Persistence.Repositories;

public sealed class JobRepository : BaseRepository, IJobRepository
{
    public JobRepository(GeminiWarehouseDbContext dbContext) : base(dbContext)
    {
    }

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        await _context.Jobs.AddAsync(job, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}