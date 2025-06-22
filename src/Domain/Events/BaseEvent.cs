namespace Domain.Events;

public abstract record BaseEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public long Version { get; set; }
    public string AggregateId { get; set; } = string.Empty;
    public string EventType { get; init; } = string.Empty;

    protected BaseEvent()
    {
        EventType = GetType().Name;
    }
}
