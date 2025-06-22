using Domain.Aggregates;
using Domain.Events;
using Domain.ValueObjects;
using Infrastructure.EventStore;

namespace Infrastructure.Repositories;

public class AccountRepository
{
    private readonly IEventStore _eventStore;

    public AccountRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Account?> GetByIdAsync(
        AccountId accountId,
        CancellationToken cancellationToken = default
    )
    {
        var events = await _eventStore.GetEventsAsync(accountId.Value, cancellationToken);
        if (!events.Any())
            return null;

        var account = new Account();
        account.LoadFromHistory(events);
        return account;
    }

    public async Task SaveAsync(Account account, CancellationToken cancellationToken = default)
    {
        var uncommittedEvents = account.UncommittedEvents.ToList();
        if (!uncommittedEvents.Any())
            return;

        await _eventStore.AppendEventsAsync(
            account.Id.Value,
            account.Version - uncommittedEvents.Count,
            uncommittedEvents,
            cancellationToken
        );
        account.MarkEventsAsCommitted();
    }

    public async Task<IEnumerable<BaseEvent>> GetEventsUpToAsync(
        AccountId accountId,
        DateTimeOffset upToDate,
        CancellationToken cancellationToken = default
    )
    {
        var allEvents = await _eventStore.GetEventsAsync(accountId.Value, cancellationToken);
        return allEvents.Where(e => e.Timestamp <= upToDate);
    }
}
