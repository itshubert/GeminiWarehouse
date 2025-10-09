namespace GeminiWarehouse.Application.Common.Messaging;

public interface IEventBridgePublisher
{
    Task PublishAsync<T>(
        DetailTypes detailType,
        T eventDetail,
        CancellationToken cancellationToken = default) where T : class;
}
