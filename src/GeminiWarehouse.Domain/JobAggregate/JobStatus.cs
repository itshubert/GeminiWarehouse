namespace GeminiWarehouse.Domain.JobAggregate;

public enum JobStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}