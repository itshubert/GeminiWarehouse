using GeminiWarehouse.Application.Common.Models.Jobs;
using GeminiWarehouse.Domain.JobAggregate;
using GeminiWarehouse.Domain.JobAggregate.Entities;
using GeminiWarehouse.Domain.JobAggregate.ValueObjects;
using Mapster;

namespace GeminiWarehouse.Application.Common.Mappings;

public sealed class JobMappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Job, JobModel>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest, src => src);

        config.NewConfig<JobItem, JobItemModel>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.JobId, src => src.JobId.Value)
            .Map(dest => dest, src => src);
    }
}