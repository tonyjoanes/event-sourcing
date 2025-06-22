using Domain.ValueObjects;

namespace Domain.Events;

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

public record MoneyWithdrawn : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required Money Amount { get; init; }
    public required string Description { get; init; }
    public DateTimeOffset WithdrawnAt { get; init; }
}

public record MoneyTransferred : BaseEvent
{
    public required AccountId FromAccountId { get; init; }
    public required AccountId ToAccountId { get; init; }
    public required Money Amount { get; init; }
    public required string Description { get; init; }
    public DateTimeOffset TransferredAt { get; init; }
}

public record AccountFrozen : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required string Reason { get; init; }
    public DateTimeOffset FrozenAt { get; init; }
}

public record AccountUnfrozen : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public DateTimeOffset UnfrozenAt { get; init; }
}

public record AccountClosed : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required string Reason { get; init; }
    public DateTimeOffset ClosedAt { get; init; }
}

public record OverdraftFeeCharged : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required Money FeeAmount { get; init; }
    public DateTimeOffset ChargedAt { get; init; }
}

public record InterestAccrued : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required Money InterestAmount { get; init; }
    public DateTimeOffset AccruedAt { get; init; }
}

// New events for enhanced functionality
public record DailyWithdrawalLimitExceeded : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required Money AttemptedAmount { get; init; }
    public required Money DailyLimit { get; init; }
    public DateTimeOffset AttemptedAt { get; init; }
}

public record MinimumBalanceViolation : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required Money CurrentBalance { get; init; }
    public required Money MinimumBalance { get; init; }
    public DateTimeOffset ViolatedAt { get; init; }
}

public record ComplianceViolationDetected : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required string ViolationType { get; init; }
    public required string Reason { get; init; }
    public required Money TransactionAmount { get; init; }
    public DateTimeOffset DetectedAt { get; init; }
}

public record AccountLimitsUpdated : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required AccountLimits OldLimits { get; init; }
    public required AccountLimits NewLimits { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record TransactionReversed : BaseEvent
{
    public required AccountId AccountId { get; init; }
    public required TransactionId OriginalTransactionId { get; init; }
    public required Money ReversedAmount { get; init; }
    public required string Reason { get; init; }
    public DateTimeOffset ReversedAt { get; init; }
}
