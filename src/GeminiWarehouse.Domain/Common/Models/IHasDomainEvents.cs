namespace GeminiWarehouse.Domain.Common.Models;

public interface IHasDomainEvents
{
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }

    public void AddDomainEvent(IDomainEvent domainEvent);
    public void ClearDomainEvents();
}