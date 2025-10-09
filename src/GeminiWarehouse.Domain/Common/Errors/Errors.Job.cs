using ErrorOr;

namespace GeminiWarehouse.Domain.Common.Errors;

public static partial class Errors
{
    public static class Job
    {
        public static Error NotFound => Error.NotFound(
            code: "Fulfillment.NotFound",
            description: "The specified order fulfillment was not found.");

        public static Error JobFulfillmentAlreadyExists(Guid fulfillmentId) => Error.Conflict(
            code: "Job.FulfillmentAlreadyExists",
            description: $"A job with fulfillment ID '{fulfillmentId}' already exists.");

        public static Error JobOrderAlreadyExists(Guid orderId) => Error.Conflict(
            code: "Job.OrderAlreadyExists",
            description: $"A job with Order ID '{orderId}' already exists.");
    }
}