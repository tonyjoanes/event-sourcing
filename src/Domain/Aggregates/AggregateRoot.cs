using Domain.Events;

namespace Domain.Aggregates;

public abstract class AggregateRoot
{
    private readonly List<BaseEvent> _uncommittedEvents = new();
    private long _version = 0;

    public string Id { get; protected set; } = string.Empty;
    public long Version => _version;

    public IReadOnlyCollection<BaseEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    protected void Apply(BaseEvent @event)
    {
        @event.AggregateId = Id;
        @event.Version = _version + 1;

        When(@event);
        _uncommittedEvents.Add(@event);
        _version++;
    }

    protected abstract void When(BaseEvent @event);

    public void LoadFromHistory(IEnumerable<BaseEvent> events)
    {
        foreach (var @event in events)
        {
            When(@event);
            _version++;
        }
    }

    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    protected virtual void EnsureValidState()
    {
        // Override in derived classes to implement business rule validation
    }
}
