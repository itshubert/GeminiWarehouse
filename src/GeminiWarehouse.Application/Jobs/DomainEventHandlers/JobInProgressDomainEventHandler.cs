using GeminiWarehouse.Application.Common.Interfaces;
using GeminiWarehouse.Application.Common.Messaging;
using GeminiWarehouse.Domain.JobAggregate.Events;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiWarehouse.Application.Jobs.DomainEventHandlers;

public sealed record JobInProgressDomainEventHandler : INotificationHandler<JobInProgressDomainEvent>
{
    private readonly IJobRepository _jobRepository;
    private readonly IEventBridgePublisher _eventBridgePublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<JobInProgressDomainEventHandler> _logger;

    public JobInProgressDomainEventHandler(
        IJobRepository jobRepository,
        IEventBridgePublisher eventBridgePublisher,
        IMapper mapper,
        ILogger<JobInProgressDomainEventHandler> logger)
    {
        _jobRepository = jobRepository;
        _eventBridgePublisher = eventBridgePublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task Handle(JobInProgressDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Publishing PickInProgressEvent Job with ID {notification.JobId} is now in progress.");

        var job = await _jobRepository.GetByIdForUpdateAsync(notification.JobId, cancellationToken);

        if (job is null)
        {
            _logger.LogWarning($"Job with ID {notification.JobId} not found.");
            return;
        }

        await _eventBridgePublisher.PublishAsync(DetailTypes.JobPickInProgress, new { JobId = notification.JobId.Value, job.OrderId }, cancellationToken);
    }
}