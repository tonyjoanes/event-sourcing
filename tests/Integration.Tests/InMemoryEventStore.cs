using System.Collections.Concurrent;
using Domain.Events;
using Infrastructure.EventStore;

namespace Integration.Tests;

public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<string, List<BaseEvent>> _store = new();

    public Task<long> AppendEventsAsync(
        string aggregateId,
        long expectedVersion,
        IEnumerable<BaseEvent> events,
        CancellationToken cancellationToken = default
    )
    {
        var eventList = events.ToList();
        if (!_store.TryGetValue(aggregateId, out var existing))
        {
            existing = new List<BaseEvent>();
            _store[aggregateId] = existing;
        }
        if (existing.Count != expectedVersion)
            throw new InvalidOperationException(
                $"Concurrency conflict: expected version {expectedVersion}, actual {existing.Count}"
            );
        long version = expectedVersion;
        foreach (var e in eventList)
        {
            version++;
            e.Version = version;
            e.AggregateId = aggregateId;
            existing.Add(e);
        }
        return Task.FromResult(version);
    }

    public Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        CancellationToken cancellationToken = default
    )
    {
        if (_store.TryGetValue(aggregateId, out var events))
            return Task.FromResult(events.OrderBy(e => e.Version).AsEnumerable());
        return Task.FromResult(Enumerable.Empty<BaseEvent>());
    }

    public Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        long fromVersion,
        CancellationToken cancellationToken = default
    )
    {
        if (_store.TryGetValue(aggregateId, out var events))
            return Task.FromResult(
                events.Where(e => e.Version >= fromVersion).OrderBy(e => e.Version).AsEnumerable()
            );
        return Task.FromResult(Enumerable.Empty<BaseEvent>());
    }

    public Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        long fromVersion,
        long toVersion,
        CancellationToken cancellationToken = default
    )
    {
        if (_store.TryGetValue(aggregateId, out var events))
            return Task.FromResult(
                events
                    .Where(e => e.Version >= fromVersion && e.Version <= toVersion)
                    .OrderBy(e => e.Version)
                    .AsEnumerable()
            );
        return Task.FromResult(Enumerable.Empty<BaseEvent>());
    }

    public Task<long> GetCurrentVersionAsync(
        string aggregateId,
        CancellationToken cancellationToken = default
    )
    {
        if (_store.TryGetValue(aggregateId, out var events))
            return Task.FromResult((long)events.Count);
        return Task.FromResult(0L);
    }

    public Task<bool> ExistsAsync(string aggregateId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_store.ContainsKey(aggregateId));
    }
}
