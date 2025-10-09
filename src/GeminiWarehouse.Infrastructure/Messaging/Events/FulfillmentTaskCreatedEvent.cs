using GeminiWarehouse.Application.Common.Models.Jobs;

namespace GeminiWarehouse.Infrastructure.Messaging.Events;

public sealed record FulfillmentTaskCreatedEvent(
    Guid FulfillmentId,
    Guid OrderId,
    string Status,
    string? TrackingNumber,
    ShippingAddressModel ShippingAddress,
    List<JobItemModel> JobItems);