using Domain.Events;
using Newtonsoft.Json;
using Raven.Client.Documents;

namespace Infrastructure.EventStore;

public class RavenDbEventStore : IEventStore
{
    private readonly IDocumentStore _documentStore;
    private readonly JsonSerializerSettings _jsonSettings;

    public RavenDbEventStore(IDocumentStore documentStore)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));

        _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };
    }

    public async Task<long> AppendEventsAsync(
        string aggregateId,
        long expectedVersion,
        IEnumerable<BaseEvent> events,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
            throw new ArgumentException(
                "Aggregate ID cannot be null or empty",
                nameof(aggregateId)
            );

        if (events == null || !events.Any())
            throw new ArgumentException("Events cannot be null or empty", nameof(events));

        using var session = _documentStore.OpenAsyncSession();

        // Check current version for optimistic concurrency
        var currentVersion = await GetCurrentVersionAsync(aggregateId, cancellationToken);
        if (currentVersion != expectedVersion)
        {
            throw new ConcurrencyException(
                $"Expected version {expectedVersion} but found {currentVersion} for aggregate {aggregateId}"
            );
        }

        var eventList = events.ToList();
        var newVersion = expectedVersion;

        foreach (var @event in eventList)
        {
            newVersion++;
            @event.Version = newVersion;
            @event.AggregateId = aggregateId;

            var eventDocument = new EventDocument
            {
                Id = $"{aggregateId}/{newVersion}",
                AggregateId = aggregateId,
                Version = newVersion,
                EventType = @event.GetType().Name,
                EventData = JsonConvert.SerializeObject(@event, _jsonSettings),
                Timestamp = @event.Timestamp,
            };

            await session.StoreAsync(eventDocument, cancellationToken);
        }

        await session.SaveChangesAsync(cancellationToken);
        return newVersion;
    }

    public async Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        CancellationToken cancellationToken = default
    )
    {
        return await GetEventsAsync(aggregateId, 1, long.MaxValue, cancellationToken);
    }

    public async Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        long fromVersion,
        CancellationToken cancellationToken = default
    )
    {
        return await GetEventsAsync(aggregateId, fromVersion, long.MaxValue, cancellationToken);
    }

    public async Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        long fromVersion,
        long toVersion,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
            throw new ArgumentException(
                "Aggregate ID cannot be null or empty",
                nameof(aggregateId)
            );

        using var session = _documentStore.OpenAsyncSession();

        var eventDocuments = await session
            .Query<EventDocument>()
            .Where(e =>
                e.AggregateId == aggregateId && e.Version >= fromVersion && e.Version <= toVersion
            )
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        var events = new List<BaseEvent>();

        foreach (var eventDoc in eventDocuments)
        {
            var eventType = Type.GetType(eventDoc.EventType);
            if (eventType == null)
            {
                // Try to find the type in the current assembly
                eventType = typeof(BaseEvent)
                    .Assembly.GetTypes()
                    .FirstOrDefault(t =>
                        t.Name == eventDoc.EventType && typeof(BaseEvent).IsAssignableFrom(t)
                    );
            }

            if (eventType == null)
            {
                throw new InvalidOperationException(
                    $"Could not find event type: {eventDoc.EventType}"
                );
            }

            var @event =
                JsonConvert.DeserializeObject(eventDoc.EventData, eventType, _jsonSettings)
                as BaseEvent;
            if (@event == null)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize event: {eventDoc.EventType}"
                );
            }

            events.Add(@event);
        }

        return events;
    }

    public async Task<long> GetCurrentVersionAsync(
        string aggregateId,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
            throw new ArgumentException(
                "Aggregate ID cannot be null or empty",
                nameof(aggregateId)
            );

        using var session = _documentStore.OpenAsyncSession();

        var latestEvent = await session
            .Query<EventDocument>()
            .Where(e => e.AggregateId == aggregateId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return latestEvent?.Version ?? 0;
    }

    public async Task<bool> ExistsAsync(
        string aggregateId,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
            throw new ArgumentException(
                "Aggregate ID cannot be null or empty",
                nameof(aggregateId)
            );

        using var session = _documentStore.OpenAsyncSession();

        var exists = await session
            .Query<EventDocument>()
            .Where(e => e.AggregateId == aggregateId)
            .AnyAsync(cancellationToken);

        return exists;
    }
}

public class EventDocument
{
    public string Id { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public long Version { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message)
        : base(message) { }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException) { }
}
