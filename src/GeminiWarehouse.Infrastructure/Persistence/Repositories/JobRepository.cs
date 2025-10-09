using GeminiWarehouse.Application.Common.Interfaces;
using GeminiWarehouse.Domain.JobAggregate;
using Microsoft.EntityFrameworkCore;

namespace GeminiWarehouse.Infrastructure.Persistence.Repositories;

public sealed class JobRepository : BaseRepository, IJobRepository
{
    public JobRepository(GeminiWarehouseDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Job?> GetJobByFulfillmentIdAsync(Guid fulfillmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.FulfillmentId == fulfillmentId, cancellationToken);
    }

    public async Task<Job?> GetJobByOrderIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.OrderId == jobId, cancellationToken);
    }

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        await _context.Jobs.AddAsync(job, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}