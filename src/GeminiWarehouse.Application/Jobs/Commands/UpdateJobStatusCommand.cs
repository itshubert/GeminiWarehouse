using ErrorOr;
using GeminiWarehouse.Application.Common.Interfaces;
using GeminiWarehouse.Application.Common.Models.Jobs;
using GeminiWarehouse.Domain.JobAggregate.ValueObjects;
using MediatR;

namespace GeminiWarehouse.Application.Jobs.Commands;

public sealed record UpdateJobStatusCommand(Guid JobId, JobStatus NewStatus) : IRequest<ErrorOr<Success>>;

public sealed class UpdateJobStatusCommandHandler : IRequestHandler<UpdateJobStatusCommand, ErrorOr<Success>>
{
    private readonly IJobRepository _jobRepository;

    public UpdateJobStatusCommandHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<ErrorOr<Success>> Handle(UpdateJobStatusCommand request, CancellationToken cancellationToken)
    {
        var jobId = JobId.Create(request.JobId);
        var job = await _jobRepository.GetByIdForUpdateAsync(jobId, cancellationToken);

        if (job is null)
        {
            return Error.NotFound(description: $"Job with ID {request.JobId} not found.");
        }

        job.UpdateStatus((Domain.JobAggregate.JobStatus)request.NewStatus);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}