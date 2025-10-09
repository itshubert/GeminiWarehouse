namespace GeminiWarehouse.Application.Common.Messaging;

public interface IEventProcessor<TEvent>
{
    Task<bool> ProcessEventAsync(TEvent @event, CancellationToken cancellationToken);
}