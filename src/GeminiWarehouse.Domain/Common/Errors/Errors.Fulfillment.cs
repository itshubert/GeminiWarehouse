using ErrorOr;

namespace GeminiOrderFulfillment.Domain.Common.Errors;

public static partial class Errors
{
    public static class Fulfillment
    {
        public static Error NotFound => Error.NotFound(
            code: "Fulfillment.NotFound",
            description: "The specified order fulfillment was not found.");

        public static Error InvalidOrderId(Guid orderId) => Error.Validation(
            code: "Fulfillment.InvalidOrderId",
            description: $"The fulfillment with Order ID '{orderId}' is invalid or does not exist.");

        public static Error FulfillmentAlreadyExists(Guid orderId) => Error.Conflict(
            code: "Fulfillment.AlreadyExists",
            description: $"A fulfillment for Order ID '{orderId}' already exists.");
    }
}