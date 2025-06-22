# Event Sourcing: A Comprehensive Guide with Real-World Implementation

Event sourcing is an architectural pattern that fundamentally changes how we think about data persistence and system design. Rather than storing just the current state of our entities, event sourcing captures every change as an immutable event, creating a complete audit trail of what happened in our system over time.

## What is Event Sourcing?

Think of event sourcing like a bank ledger. Instead of just showing your current balance ($1,250), a traditional ledger records every transaction that led to that balance:
- Opened account with $1,000
- Deposited $500  
- Withdrew $250

In event sourcing, we apply this same principle to all our data. Rather than storing just the current state of our domain entities, we store a sequence of events that represent all the changes that have occurred. The current state is derived by replaying these events from the beginning.

## Core Event Sourcing Concepts

### Events as First-Class Citizens

In event sourcing, events are immutable facts about what happened in your system. Here's how we define events in our banking demo:

```12:16:src/Domain/Events/BaseEvent.cs
public abstract record BaseEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public long Version { get; set; }
    public string AggregateId { get; set; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
}
```

Domain events capture specific business occurrences:

```5:21:src/Domain/Events/AccountEvents.cs
public record AccountOpened : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required CustomerId CustomerId { get; init; }
    public required Money InitialBalance { get; init; }
    public DateTimeOffset OpenedAt { get; init; }
}

public record MoneyDeposited : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required Money Amount { get; init; }
    public required string Description { get; init; }
    public DateTimeOffset DepositedAt { get; init; }
}
```

Each event captures:
- **What happened** (MoneyDeposited, AccountOpened)
- **When it happened** (Timestamp)
- **Which entity it affected** (AggregateId)
- **All relevant data** about the change

### Aggregates: The Guardians of Business Rules

Aggregates in event sourcing are entities that maintain consistency boundaries and enforce business rules. Our `Account` aggregate demonstrates this pattern:

```50:77:src/Domain/Aggregates/Account.cs
    public OneOf<Success, InsufficientFunds, AccountFrozenError, ValidationError> Withdraw(
        Money amount,
        Option<string> description = null
    )
    {
        if (amount.Amount <= 0)
            return new ValidationError("Withdrawal amount must be positive");

        if (Status != AccountStatus.Active)
            return new AccountFrozenError($"Account {Id} is {Status}");

        if (Balance < amount)
            return new InsufficientFunds(
                $"Insufficient funds. Balance: {Balance}, Requested: {amount}"
            );

        Apply(
            new MoneyWithdrawn
            {
                AccountId = Id,
                Amount = amount,
                Description = description?.GetValueOrDefault("Withdrawal") ?? "Withdrawal",
                WithdrawnAt = DateTimeOffset.UtcNow,
            }
        );

        return new Success();
    }
```

The `AggregateRoot` base class provides the infrastructure for event handling:

```5:35:src/Domain/Aggregates/AggregateRoot.cs
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
}
```

Key principles demonstrated:
1. **Business rules are enforced** before events are created
2. **State changes happen through events** using the `Apply()` method
3. **Immutable events** represent successful business operations
4. **Version tracking** enables optimistic concurrency control

### Event Store: The Source of Truth

The event store is your system's single source of truth:

```5:30:src/Infrastructure/EventStore/IEventStore.cs
public interface IEventStore
{
    /// <summary>
    /// Appends events to the event stream for a specific aggregate
    /// </summary>
    Task<long> AppendEventsAsync(
        string aggregateId,
        long expectedVersion,
        IEnumerable<BaseEvent> events,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all events for a specific aggregate
    /// </summary>
    Task<IEnumerable<BaseEvent>> GetEventsAsync(
        string aggregateId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the current version of an aggregate
    /// </summary>
    Task<long> GetCurrentVersionAsync(
        string aggregateId,
        CancellationToken cancellationToken = default
    );
}
```

The event store provides:
- **Append-only writes** for new events
- **Optimistic concurrency control** through versioning
- **Event retrieval** for aggregate reconstruction
- **Atomic transactions** within aggregate boundaries

## CQRS: The Perfect Partner

Event sourcing naturally pairs with Command Query Responsibility Segregation (CQRS), where commands modify state and queries read state from separate models.

### Commands and Command Handlers

Commands represent intent to change the system:

```5:11:src/Application/Commands/OpenAccountCommand.cs
public record OpenAccountCommand
{
    public required CustomerId CustomerId { get; init; }
    public required Money InitialBalance { get; init; }
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
}
```

Command handlers orchestrate the business operation:

```22:50:src/Application/Handlers/OpenAccountCommandHandler.cs
    public async Task<OpenAccountCommandResult> HandleAsync(OpenAccountCommand command)
    {
        try
        {
            // Create account using domain logic
            var accountResult = Account.Open(command.CustomerId, command.InitialBalance);

            if (accountResult.IsT1) // ValidationError
            {
                return new CommandFailure(accountResult.AsT1.Message);
            }

            var account = accountResult.AsT0;

            // Collect events before saving (since SaveAsync clears them)
            var eventsToDispatch = account.UncommittedEvents.ToList();

            // Save to event store
            await _accountRepository.SaveAsync(account);

            // Publish events to update read models
            foreach (var @event in eventsToDispatch)
            {
                await _eventDispatcher.DispatchAsync(@event);
            }

            return new CommandSuccess<OpenAccountResult>(result);
        }
        catch (Exception ex)
        {
            return new CommandFailure($"Failed to open account: {ex.Message}");
        }
    }
```

### Read Models and Projections

Read models are optimized for specific query patterns:

```3:17:src/Infrastructure/ReadModels/Projections/AccountSummaryProjection.cs
public class AccountSummaryProjection
{
    public string Id { get; set; } = default!; // AccountId
    public string CustomerId { get; set; } = default!;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTimeOffset? LastTransactionAt { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal OverdraftLimit { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MinimumBalance { get; set; }
}
```

Projection handlers update read models when events occur:

```35:48:src/Infrastructure/ReadModels/Projections/AccountSummaryProjectionHandler.cs
    public async Task HandleAsync(MoneyDeposited @event)
    {
        var accountSummary = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.AccountId.Value
        );
        if (accountSummary == null)
            return;

        accountSummary.Balance += @event.Amount.Amount;
        accountSummary.LastTransactionAt = @event.DepositedAt;

        await _readModelStore.UpdateAsync(accountSummary);
    }
```

The event dispatcher routes events to appropriate handlers:

```18:50:src/Infrastructure/ReadModels/Projections/EventDispatcher.cs
    public async Task DispatchAsync(BaseEvent @event)
    {
        // Get projection handlers
        var accountSummaryHandler = _serviceProvider.GetRequiredService<AccountSummaryProjectionHandler>();
        var transactionHistoryHandler = _serviceProvider.GetRequiredService<TransactionHistoryProjectionHandler>();

        // Route events to appropriate handlers
        switch (@event)
        {
            case AccountOpened accountOpened:
                await accountSummaryHandler.HandleAsync(accountOpened);
                break;

            case MoneyDeposited moneyDeposited:
                await accountSummaryHandler.HandleAsync(moneyDeposited);
                await transactionHistoryHandler.HandleAsync(moneyDeposited);
                break;

            case MoneyWithdrawn moneyWithdrawn:
                await accountSummaryHandler.HandleAsync(moneyWithdrawn);
                await transactionHistoryHandler.HandleAsync(moneyWithdrawn);
                break;

            case MoneyTransferred moneyTransferred:
                await accountSummaryHandler.HandleAsync(moneyTransferred);
                await transactionHistoryHandler.HandleAsync(moneyTransferred);
                break;
        }
    }
```

## Key Benefits of Event Sourcing

### 1. Complete Audit Trail

Every change to your system is permanently recorded. Our banking demo shows this with the events endpoint:

```138:160:src/WebApi/Controllers/AccountController.cs
    [HttpGet("{accountId}/events")]
    public async Task<IActionResult> GetAccountEvents(string accountId)
    {
        try
        {
            var eventStore = HttpContext.RequestServices.GetRequiredService<IEventStore>();
            var events = await eventStore.GetEventsAsync(new AccountId(accountId));

            return Ok(
                new
                {
                    AccountId = accountId,
                    EventCount = events.Count(),
                    Events = events.Select(e => new
                    {
                        Type = e.GetType().Name,
                        Version = e.Version,
                        Timestamp = e.Timestamp,
                        Data = e,
                    }),
                }
            );
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = $"Failed to retrieve events: {ex.Message}" });
        }
    }
```

### 2. Temporal Queries

You can answer questions like "What was the balance on January 15th?":

```114:125:src/WebApi/Controllers/AccountController.cs
    [HttpGet("{accountId}/balance-at")]
    public async Task<IActionResult> GetBalanceAt(string accountId, [FromQuery] DateTimeOffset date)
    {
        var result = await _queryService.GetBalanceAtAsync(new AccountId(accountId), date);

        return result.Match<IActionResult>(
            success => Ok(new { Balance = success.Data.Balance.Amount, Date = date }),
            failure => NotFound(new { Error = failure.Reason })
        );
    }
```

### 3. Business Intelligence and Analytics

Events provide rich data for analysis:
- Customer transaction patterns
- Peak usage times
- Fraud detection through anomaly patterns
- Regulatory compliance reporting

### 4. System Evolution and Migration

Need to change business logic? With event sourcing, you can:
1. Create new projection handlers with updated logic
2. Replay all historical events through the new handlers
3. Compare results with existing projections
4. Switch over when confident in the new calculations

## Implementation Considerations

### Performance Optimization

**Event Snapshots**: For aggregates with many events, create periodic snapshots to avoid replaying thousands of events.

**Read Model Optimization**: Design read models for specific query patterns rather than trying to create generic models.

### Eventual Consistency

Event sourcing systems are eventually consistent. Commands succeed immediately, but read models update asynchronously. Design your UI to handle this gracefully with optimistic updates or loading states.

### Event Schema Evolution

Events are permanent, so plan for schema changes:

```csharp
// Version 1
public record MoneyWithdrawn : BaseEvent
{
    public required Money Amount { get; init; }
}

// Version 2 - Add optional fee
public record MoneyWithdrawn : BaseEvent  
{
    public required Money Amount { get; init; }
    public Money? Fee { get; init; } // New optional field
}
```

Use versioning strategies:
- **Additive changes**: Add optional fields
- **Breaking changes**: Create new event types
- **Event upcasting**: Convert old events to new formats during replay

## When to Use Event Sourcing

### Ideal Use Cases

**Financial Systems**: Banking, payment processing, trading platforms
- Regulatory requirements for audit trails
- Need for precise transaction history
- Complex business rules around money

**Collaborative Systems**: Document editing, project management
- Multiple users making concurrent changes
- Need to track who changed what when
- Ability to replay or undo changes

**Workflow Systems**: Order processing, case management
- Complex state transitions
- Need to understand decision history
- Regulatory or compliance requirements

### When to Avoid Event Sourcing

**Simple CRUD Applications**: Basic content management, user profiles
- Simple state changes without complex business logic
- No audit requirements beyond basic logging
- Performance is critical and eventual consistency is problematic

**Reporting-Heavy Systems**: Analytics dashboards, data warehouses
- Read-heavy workloads with simple write patterns
- Data aggregation is the primary concern
- Real-time reporting requirements

**Systems with Rapidly Changing Requirements**: Early-stage products, prototypes
- Event schemas are hard to change
- Unknown query patterns make read model design difficult
- Team lacks event sourcing experience

## Common Pitfalls and How to Avoid Them

### 1. Events That Are Too Granular

**Problem**: Creating events for every property change

**Solution**: Model business-meaningful events that represent real business occurrences, like our banking demo's events:

```28:36:src/Domain/Events/AccountEvents.cs
public record MoneyTransferred : BaseEvent
{
    public required AccountId FromAccountId { get; init; }
    public required AccountId ToAccountId { get; init; }
    public required Money Amount { get; init; }
    public required string Description { get; init; }
    public DateTimeOffset TransferredAt { get; init; }
}
```

### 2. Putting Business Logic in Event Handlers

**Problem**: Processing business rules in projection handlers

**Solution**: Keep projections simple, put business logic in aggregates. Projections should only update read models, not enforce business rules.

### 3. Synchronous Read Model Updates

**Problem**: Waiting for all read models to update before returning from commands

**Solution**: Embrace eventual consistency and design UIs accordingly

### 4. Not Planning for Event Store Growth

**Problem**: Unlimited event storage without archival strategy

**Solution**: 
- Implement event archival policies
- Use snapshots for long-lived aggregates  
- Consider partitioning strategies for high-volume aggregates

## Getting Started: Practical Next Steps

### 1. Start Small
Begin with a bounded context that has:
- Clear business events
- Audit requirements  
- Manageable complexity

### 2. Build the Core Components
Implement these in order:
1. **Event definitions** - Start with 3-5 key events
2. **Basic aggregate** - One aggregate with essential business rules
3. **Simple event store** - In-memory or file-based for prototyping
4. **One read model** - For the most important query

### 3. Evolve Gradually
- Add more events as you understand the domain better
- Create additional read models for specific query needs
- Optimize the event store when performance becomes important
- Add snapshots when aggregates have many events

### 4. Learn from the Banking Demo

Our banking demo provides a complete, working example of event sourcing with:
- Domain-driven design principles
- CQRS implementation
- Multiple read models
- RESTful API integration
- Comprehensive event types including advanced scenarios:

```73:98:src/Domain/Events/AccountEvents.cs
public record DailyWithdrawalLimitExceeded : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required Money AttemptedAmount { get; init; }
    public required Money DailyLimit { get; init; }
    public DateTimeOffset AttemptedAt { get; init; }
}

public record ComplianceViolationDetected : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required string ViolationType { get; init; }
    public required string Reason { get; init; }
    public required Money TransactionAmount { get; init; }
    public DateTimeOffset DetectedAt { get; init; }
}

public record TransactionReversed : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required TransactionId OriginalTransactionId { get; init; }
    public required Money ReversedAmount { get; init; }
    public required string Reason { get; init; }
    public DateTimeOffset ReversedAt { get; init; }
}
```

Key endpoints to explore:
- `POST /api/account/open` - See command processing
- `GET /api/account/{id}/events` - View the event stream
- `GET /api/account/{id}/balance-at?date=2024-01-01` - Experience temporal queries
- `GET /api/account/{id}/summary` - Understand read model projections

## Conclusion

Event sourcing is a powerful architectural pattern that provides unprecedented visibility into your system's behavior and history. While it introduces complexity around eventual consistency and event schema management, the benefits of complete audit trails, temporal queries, and system evolution capabilities make it invaluable for many business domains.

The key to successful event sourcing adoption is starting simple, focusing on clear business events, and gradually building expertise with the pattern. Use the banking demo as a reference implementation, but adapt the patterns to your specific domain and requirements.

Remember: event sourcing isn't about technology—it's about modeling your business as a series of facts about what happened. When you get that right, the technical implementation follows naturally, and you'll have a system that truly serves your business needs both today and in the future.

## Additional Resources

- **Repository**: The complete banking demo with comprehensive tests and documentation
- **Event Store Solutions**: Consider RavenDB, EventStore, or cloud solutions like AWS EventBridge
- **CQRS Frameworks**: Explore Axon Framework, NEventStore, or build your own lightweight solution
- **Domain-Driven Design**: Essential companion knowledge for effective event sourcing implementation

Event sourcing represents a fundamental shift in how we think about data and system design. Embrace the learning curve, start with simple implementations, and gradually build the sophisticated event-driven systems that can evolve with your business needs. 