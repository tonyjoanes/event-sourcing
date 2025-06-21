using Domain.Events;

namespace Infrastructure.ReadModels.Projections;

public class TransactionHistoryProjectionHandler
{
    private readonly IReadModelStore _readModelStore;

    public TransactionHistoryProjectionHandler(IReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public async Task HandleAsync(MoneyDeposited @event)
    {
        var transaction = new TransactionHistoryProjection
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = @event.AccountId.Value,
            Type = "Deposit",
            Amount = @event.Amount.Amount,
            Currency = @event.Amount.Currency,
            Description = @event.Description,
            Timestamp = @event.DepositedAt,
            CustomerId = string.Empty // Will be populated from AccountSummary
        };

        await _readModelStore.InsertAsync(transaction);
    }

    public async Task HandleAsync(MoneyWithdrawn @event)
    {
        var transaction = new TransactionHistoryProjection
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = @event.AccountId.Value,
            Type = "Withdrawal",
            Amount = @event.Amount.Amount,
            Currency = @event.Amount.Currency,
            Description = @event.Description,
            Timestamp = @event.WithdrawnAt,
            CustomerId = string.Empty // Will be populated from AccountSummary
        };

        await _readModelStore.InsertAsync(transaction);
    }

    public async Task HandleAsync(MoneyTransferred @event)
    {
        // Create transaction record for source account
        var sourceTransaction = new TransactionHistoryProjection
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = @event.FromAccountId.Value,
            Type = "TransferOut",
            Amount = @event.Amount.Amount,
            Currency = @event.Amount.Currency,
            Description = $"Transfer to {@event.ToAccountId.Value}: {@event.Description}",
            Timestamp = @event.TransferredAt,
            RelatedAccountId = @event.ToAccountId.Value,
            CustomerId = string.Empty // Will be populated from AccountSummary
        };

        await _readModelStore.InsertAsync(sourceTransaction);

        // Create transaction record for destination account
        var destinationTransaction = new TransactionHistoryProjection
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = @event.ToAccountId.Value,
            Type = "TransferIn",
            Amount = @event.Amount.Amount,
            Currency = @event.Amount.Currency,
            Description = $"Transfer from {@event.FromAccountId.Value}: {@event.Description}",
            Timestamp = @event.TransferredAt,
            RelatedAccountId = @event.FromAccountId.Value,
            CustomerId = string.Empty // Will be populated from AccountSummary
        };

        await _readModelStore.InsertAsync(destinationTransaction);
    }

    public async Task HandleAsync(OverdraftFeeCharged @event)
    {
        var transaction = new TransactionHistoryProjection
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = @event.AccountId.Value,
            Type = "OverdraftFee",
            Amount = @event.FeeAmount.Amount,
            Currency = @event.FeeAmount.Currency,
            Description = "Overdraft fee charge",
            Timestamp = @event.ChargedAt,
            CustomerId = string.Empty // Will be populated from AccountSummary
        };

        await _readModelStore.InsertAsync(transaction);
    }

    public async Task HandleAsync(InterestAccrued @event)
    {
        var transaction = new TransactionHistoryProjection
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = @event.AccountId.Value,
            Type = "InterestAccrual",
            Amount = @event.InterestAmount.Amount,
            Currency = @event.InterestAmount.Currency,
            Description = "Interest accrued",
            Timestamp = @event.AccruedAt,
            CustomerId = string.Empty // Will be populated from AccountSummary
        };

        await _readModelStore.InsertAsync(transaction);
    }

    public async Task HandleAsync(ComplianceViolationDetected @event)
    {
        var transaction = new TransactionHistoryProjection
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = @event.AccountId.Value,
            Type = "ComplianceViolation",
            Amount = @event.TransactionAmount.Amount,
            Currency = @event.TransactionAmount.Currency,
            Description = $"Compliance violation: {@event.ViolationType} - {@event.Reason}",
            Timestamp = @event.DetectedAt,
            CustomerId = string.Empty // Will be populated from AccountSummary
        };

        await _readModelStore.InsertAsync(transaction);
    }

    public async Task HandleAsync(DailyWithdrawalLimitExceeded @event)
    {
        var transaction = new TransactionHistoryProjection
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = @event.AccountId.Value,
            Type = "DailyLimitExceeded",
            Amount = @event.AttemptedAmount.Amount,
            Currency = @event.AttemptedAmount.Currency,
            Description = $"Daily limit exceeded: attempted {@event.AttemptedAmount}, limit was {@event.DailyLimit}",
            Timestamp = @event.AttemptedAt,
            CustomerId = string.Empty // Will be populated from AccountSummary
        };

        await _readModelStore.InsertAsync(transaction);
    }

    public async Task HandleAsync(TransactionReversed @event)
    {
        var transaction = new TransactionHistoryProjection
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = @event.AccountId.Value,
            Type = "TransactionReversal",
            Amount = @event.ReversedAmount.Amount,
            Currency = @event.ReversedAmount.Currency,
            Description = $"Transaction reversal: {@event.Reason} (Original: {@event.OriginalTransactionId})",
            Timestamp = @event.ReversedAt,
            CustomerId = string.Empty // Will be populated from AccountSummary
        };

        await _readModelStore.InsertAsync(transaction);
    }
} 