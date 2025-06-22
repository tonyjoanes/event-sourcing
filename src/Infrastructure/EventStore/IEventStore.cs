using Domain.Events;

namespace Infrastructure.EventStore;

public interface IEventStore
{
    /// <summary>
    /// Appends events to the event stream for a specific aggregate
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate</param>
    /// <param name="expectedVersion">The expected version for optimistic concurrency control</param>
    /// <param name="events">The events to append</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The new version after appending events</returns>
    Task<long> AppendEventsAsync(
        string aggregateId,
        long expectedVersion,
        IEnumerable<BaseEvent> events,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all events for a specific aggregate
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All events for the aggregate, ordered by version</returns>
    Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves events for a specific aggregate from a specific version
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate</param>
    /// <param name="fromVersion">The version to start from (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Events from the specified version, ordered by version</returns>
    Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        long fromVersion,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves events for a specific aggregate within a version range
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate</param>
    /// <param name="fromVersion">The version to start from (inclusive)</param>
    /// <param name="toVersion">The version to end at (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Events within the specified version range, ordered by version</returns>
    Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        long fromVersion,
        long toVersion,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the current version of an aggregate
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The current version, or 0 if the aggregate doesn't exist</returns>
    Task<long> GetCurrentVersionAsync(
        string aggregateId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if an aggregate exists
    /// </summary>
    /// <param name="aggregateId">The ID of the aggregate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the aggregate exists, false otherwise</returns>
    Task<bool> ExistsAsync(string aggregateId, CancellationToken cancellationToken = default);
}
