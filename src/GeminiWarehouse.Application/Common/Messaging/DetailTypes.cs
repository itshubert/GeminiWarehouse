namespace GeminiWarehouse.Application.Common.Messaging;

// values need to much EventBridge Rule Detail Type
public enum DetailTypes
{
    JobPickInProgress,
    WarehouseJobCompleted,
    WarehouseJobCancelled
}