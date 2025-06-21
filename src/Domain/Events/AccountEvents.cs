using Domain.ValueObjects;

namespace Domain.Events;

public record AccountOpened : BaseEvent
{
    public AccountId AccountId { get; init; }
    public CustomerId CustomerId { get; init; }
    public Money InitialBalance { get; init; }
    public DateTimeOffset OpenedAt { get; init; }
}

public record MoneyDeposited : BaseEvent
{
    public AccountId AccountId { get; init; }
    public Money Amount { get; init; }
    public string Description { get; init; }
    public DateTimeOffset DepositedAt { get; init; }
}

public record MoneyWithdrawn : BaseEvent
{
    public AccountId AccountId { get; init; }
    public Money Amount { get; init; }
    public string Description { get; init; }
    public DateTimeOffset WithdrawnAt { get; init; }
}

public record MoneyTransferred : BaseEvent
{
    public AccountId FromAccountId { get; init; }
    public AccountId ToAccountId { get; init; }
    public Money Amount { get; init; }
    public string Description { get; init; }
    public DateTimeOffset TransferredAt { get; init; }
}

public record AccountFrozen : BaseEvent
{
    public AccountId AccountId { get; init; }
    public string Reason { get; init; }
    public DateTimeOffset FrozenAt { get; init; }
}

public record AccountUnfrozen : BaseEvent
{
    public AccountId AccountId { get; init; }
    public DateTimeOffset UnfrozenAt { get; init; }
}

public record AccountClosed : BaseEvent
{
    public AccountId AccountId { get; init; }
    public string Reason { get; init; }
    public DateTimeOffset ClosedAt { get; init; }
}

public record OverdraftFeeCharged : BaseEvent
{
    public AccountId AccountId { get; init; }
    public Money FeeAmount { get; init; }
    public DateTimeOffset ChargedAt { get; init; }
}

public record InterestAccrued : BaseEvent
{
    public AccountId AccountId { get; init; }
    public Money InterestAmount { get; init; }
    public DateTimeOffset AccruedAt { get; init; }
} 