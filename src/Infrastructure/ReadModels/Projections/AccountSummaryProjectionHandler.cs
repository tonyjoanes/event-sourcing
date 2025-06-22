using Domain.Events;

namespace Infrastructure.ReadModels.Projections;

public class AccountSummaryProjectionHandler
{
    private readonly IReadModelStore _readModelStore;

    public AccountSummaryProjectionHandler(IReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public async Task HandleAsync(AccountOpened @event)
    {
        var accountSummary = new AccountSummaryProjection
        {
            Id = @event.AccountId.Value,
            CustomerId = @event.CustomerId.Value,
            Balance = @event.InitialBalance.Amount,
            Currency = @event.InitialBalance.Currency,
            Status = "Active",
            OpenedAt = @event.OpenedAt,
            LastTransactionAt = @event.OpenedAt,
            OverdraftLimit = 0, // Will be updated when limits are set
            DailyLimit = 0,
            MinimumBalance = 0,
        };

        await _readModelStore.InsertAsync(accountSummary);
    }

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

    public async Task HandleAsync(MoneyWithdrawn @event)
    {
        var accountSummary = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.AccountId.Value
        );
        if (accountSummary == null)
            return;

        accountSummary.Balance -= @event.Amount.Amount;
        accountSummary.LastTransactionAt = @event.WithdrawnAt;

        await _readModelStore.UpdateAsync(accountSummary);
    }

    public async Task HandleAsync(MoneyTransferred @event)
    {
        // Update source account
        var sourceAccount = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.FromAccountId.Value
        );
        if (sourceAccount != null)
        {
            sourceAccount.Balance -= @event.Amount.Amount;
            sourceAccount.LastTransactionAt = @event.TransferredAt;
            await _readModelStore.UpdateAsync(sourceAccount);
        }

        // Update destination account
        var destinationAccount = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.ToAccountId.Value
        );
        if (destinationAccount != null)
        {
            destinationAccount.Balance += @event.Amount.Amount;
            destinationAccount.LastTransactionAt = @event.TransferredAt;
            await _readModelStore.UpdateAsync(destinationAccount);
        }
    }

    public async Task HandleAsync(AccountFrozen @event)
    {
        var accountSummary = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.AccountId.Value
        );
        if (accountSummary == null)
            return;

        accountSummary.Status = "Frozen";
        await _readModelStore.UpdateAsync(accountSummary);
    }

    public async Task HandleAsync(AccountUnfrozen @event)
    {
        var accountSummary = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.AccountId.Value
        );
        if (accountSummary == null)
            return;

        accountSummary.Status = "Active";
        await _readModelStore.UpdateAsync(accountSummary);
    }

    public async Task HandleAsync(AccountClosed @event)
    {
        var accountSummary = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.AccountId.Value
        );
        if (accountSummary == null)
            return;

        accountSummary.Status = "Closed";
        accountSummary.ClosedAt = @event.ClosedAt;
        await _readModelStore.UpdateAsync(accountSummary);
    }

    public async Task HandleAsync(OverdraftFeeCharged @event)
    {
        var accountSummary = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.AccountId.Value
        );
        if (accountSummary == null)
            return;

        accountSummary.Balance -= @event.FeeAmount.Amount;
        accountSummary.LastTransactionAt = @event.ChargedAt;
        await _readModelStore.UpdateAsync(accountSummary);
    }

    public async Task HandleAsync(InterestAccrued @event)
    {
        var accountSummary = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.AccountId.Value
        );
        if (accountSummary == null)
            return;

        accountSummary.Balance += @event.InterestAmount.Amount;
        accountSummary.LastTransactionAt = @event.AccruedAt;
        await _readModelStore.UpdateAsync(accountSummary);
    }

    public async Task HandleAsync(AccountLimitsUpdated @event)
    {
        var accountSummary = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
            @event.AccountId.Value
        );
        if (accountSummary == null)
            return;

        accountSummary.OverdraftLimit = @event.NewLimits.OverdraftLimit.Amount;
        accountSummary.DailyLimit = @event.NewLimits.DailyWithdrawalLimit.Amount;
        accountSummary.MinimumBalance = @event.NewLimits.MinimumBalance.Amount;
        await _readModelStore.UpdateAsync(accountSummary);
    }
}
