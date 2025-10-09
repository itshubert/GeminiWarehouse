using GeminiWarehouse.Application.Common.Messaging;
using GeminiWarehouse.Application.Jobs.Commands;
using GeminiWarehouse.Infrastructure.Messaging.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GeminiWarehouse.Infrastructure.Messaging.EventProcessors;

public sealed class FulfillmentTaskCreatedEventProcessor : IEventProcessor<FulfillmentTaskCreatedEvent>
{
    private IMediator _mediator;
    private readonly ILogger<FulfillmentTaskCreatedEventProcessor> _logger;

    public FulfillmentTaskCreatedEventProcessor(
        IMediator mediator,
        ILogger<FulfillmentTaskCreatedEventProcessor> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<bool> ProcessEventAsync(FulfillmentTaskCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing FulfillmentTaskCreatedEvent for FulfillmentId: {FulfillmentId}", @event.FulfillmentId);

        var command = new CreateJobCommand(
            @event.FulfillmentId,
            @event.OrderId,
            @event.Status,
            @event.TrackingNumber,
            @event.ShippingAddress,
            @event.LineItems.Select(ji => new CreateJobItemRequest(ji.ProductId, ji.ProductName, ji.Quantity)).ToList());

        return await _mediator.Send(command, cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    _logger.LogInformation("Successfully processed FulfillmentTaskCreatedEvent for FulfillmentId: {FulfillmentId}", @event.FulfillmentId);
                    return true;
                }
                else
                {
                    _logger.LogError(task.Exception, "Failed to process FulfillmentTaskCreatedEvent for FulfillmentId: {FulfillmentId}", @event.FulfillmentId);
                    return false;
                }
            }, cancellationToken);
    }
}