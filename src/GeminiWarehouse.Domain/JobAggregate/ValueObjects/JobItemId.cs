using GeminiWarehouse.Domain.Common.Models;

namespace GeminiWarehouse.Domain.JobAggregate.ValueObjects;

public sealed class JobItemId : ValueObject
{
    public Guid Value { get; }

    private JobItemId(Guid value)
    {
        Value = value;
    }

    public static JobItemId Create(Guid value)
    {
        return new JobItemId(value);
    }

    public static JobItemId CreateUnique()
    {
        return new JobItemId(Guid.NewGuid());
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}