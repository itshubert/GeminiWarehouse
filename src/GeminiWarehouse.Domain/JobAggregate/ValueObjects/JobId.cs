using GeminiWarehouse.Domain.Common.Models;

namespace GeminiWarehouse.Domain.JobAggregate.ValueObjects;

public sealed class JobId : ValueObject
{
    public Guid Value { get; private set; }

    private JobId(Guid value)
    {
        Value = value;
    }

    public static JobId CreateUnique()
    {
        return new JobId(Guid.NewGuid());
    }

    public static JobId Create(Guid value)
    {
        return new JobId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}