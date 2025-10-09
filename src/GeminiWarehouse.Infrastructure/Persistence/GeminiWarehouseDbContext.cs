using GeminiWarehouse.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace GeminiWarehouse.Infrastructure.Persistence;

public sealed class GeminiWarehouseDbContext : DbContext
{
    private readonly PublishDomainEventsInterceptor _publishDomainEventsInterceptor;

    public GeminiWarehouseDbContext(
        DbContextOptions<GeminiWarehouseDbContext> options,
        PublishDomainEventsInterceptor publishDomainEventsInterceptor
    ) : base(options)
    {
        _publishDomainEventsInterceptor = publishDomainEventsInterceptor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GeminiWarehouseDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_publishDomainEventsInterceptor);
        base.OnConfiguring(optionsBuilder);
    }
}