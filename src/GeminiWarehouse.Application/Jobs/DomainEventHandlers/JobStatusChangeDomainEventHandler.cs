using GeminiWarehouse.Application.Common.Interfaces;
using GeminiWarehouse.Application.Common.Messaging;
using GeminiWarehouse.Domain.JobAggregate.Events;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiWarehouse.Application.Jobs.DomainEventHandlers;

public sealed record JobStatusChangeDomainEventHandler : INotificationHandler<JobStatusChangeDomainEvent>
{
    private readonly IJobRepository _jobRepository;
    private readonly IEventBridgePublisher _eventBridgePublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<JobStatusChangeDomainEventHandler> _logger;

    public JobStatusChangeDomainEventHandler(
        IJobRepository jobRepository,
        IEventBridgePublisher eventBridgePublisher,
        IMapper mapper,
        ILogger<JobStatusChangeDomainEventHandler> logger)
    {
        _jobRepository = jobRepository;
        _eventBridgePublisher = eventBridgePublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Handle(JobStatusChangeDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Publishing JobStatusChangeEvent Job with ID {notification.JobId.Value} changed status to {notification.NewStatus}.");

        var job = await _jobRepository.GetByIdForUpdateAsync(notification.JobId, cancellationToken);

        if (job is null)
        {
            _logger.LogWarning($"Job with ID {notification.JobId} not found.");
            return;
        }

        switch (job.Status)
        {
            case Domain.JobAggregate.JobStatus.InProgress:
                await _eventBridgePublisher.PublishAsync(DetailTypes.JobPickInProgress, new { JobId = notification.JobId.Value, job.OrderId }, cancellationToken);
                break;
            case Domain.JobAggregate.JobStatus.Completed:
                await _eventBridgePublisher.PublishAsync(DetailTypes.WarehouseJobCompleted, new { JobId = notification.JobId.Value, job.OrderId }, cancellationToken);
                break;
            case Domain.JobAggregate.JobStatus.Cancelled:
                await _eventBridgePublisher.PublishAsync(DetailTypes.WarehouseJobCancelled, new { JobId = notification.JobId.Value, job.OrderId }, cancellationToken);
                break;
        }
    }
}