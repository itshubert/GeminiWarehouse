using GeminiWarehouse.Domain.Common.Models;
using GeminiWarehouse.Domain.JobAggregate.ValueObjects;

namespace GeminiWarehouse.Domain.JobAggregate.Events;

public sealed record JobInProgressDomainEvent(JobId JobId) : IDomainEvent;