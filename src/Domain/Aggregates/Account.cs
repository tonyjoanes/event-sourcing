using Domain.Events;
using Domain.ValueObjects;
using OneOf;

namespace Domain.Aggregates;

public class Account : AggregateRoot
{
    public new AccountId Id { get; private set; } = null!;
    public CustomerId CustomerId { get; private set; } = null!;
    public Money Balance { get; private set; } = null!;
    public AccountStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? LastTransactionAt { get; private set; }

    // Business methods
    public static OneOf<Account, ValidationError> Open(CustomerId customerId, Money initialBalance)
    {
        if (initialBalance.Amount < 0)
            return new ValidationError("Initial balance cannot be negative");

        var account = new Account();
        var accountId = AccountId.Generate();

        account.Apply(
            new AccountOpened
            {
                AccountId = accountId,
                CustomerId = customerId,
                InitialBalance = initialBalance,
                OpenedAt = DateTimeOffset.UtcNow,
            }
        );

        return account;
    }

    public OneOf<Success, InsufficientFunds, AccountFrozenError, ValidationError> Deposit(
        Money amount,
        Option<string> description = null
    )
    {
        if (amount.Amount <= 0)
            return new ValidationError("Deposit amount must be positive");

        if (Status != AccountStatus.Active)
            return new AccountFrozenError($"Account {Id} is {Status}");

        Apply(
            new MoneyDeposited
            {
                AccountId = Id,
                Amount = amount,
                Description = description?.GetValueOrDefault("Deposit") ?? "Deposit",
                DepositedAt = DateTimeOffset.UtcNow,
            }
        );

        return new Success();
    }

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

    public OneOf<Success, InsufficientFunds, AccountFrozenError, ValidationError> Transfer(
        Money amount,
        AccountId toAccountId,
        Option<string> description = null
    )
    {
        if (amount.Amount <= 0)
            return new ValidationError("Transfer amount must be positive");

        if (Status != AccountStatus.Active)
            return new AccountFrozenError($"Account {Id} is {Status}");

        if (Balance < amount)
            return new InsufficientFunds(
                $"Insufficient funds for transfer. Balance: {Balance}, Requested: {amount}"
            );

        Apply(
            new MoneyTransferred
            {
                FromAccountId = Id,
                ToAccountId = toAccountId,
                Amount = amount,
                Description = description?.GetValueOrDefault("Transfer") ?? "Transfer",
                TransferredAt = DateTimeOffset.UtcNow,
            }
        );

        return new Success();
    }

    public OneOf<Success, ValidationError> Freeze(Option<string> reason = null)
    {
        if (Status == AccountStatus.Closed)
            return new ValidationError("Cannot freeze a closed account");

        if (Status == AccountStatus.Frozen)
            return new ValidationError("Account is already frozen");

        Apply(
            new AccountFrozen
            {
                AccountId = Id,
                Reason = reason?.GetValueOrDefault("No reason provided") ?? "No reason provided",
                FrozenAt = DateTimeOffset.UtcNow,
            }
        );

        return new Success();
    }

    public OneOf<Success, ValidationError> Unfreeze()
    {
        if (Status != AccountStatus.Frozen)
            return new ValidationError("Account is not frozen");

        Apply(new AccountUnfrozen { AccountId = Id, UnfrozenAt = DateTimeOffset.UtcNow });

        return new Success();
    }

    public OneOf<Success, ValidationError> Close(Option<string> reason = null)
    {
        if (Status == AccountStatus.Closed)
            return new ValidationError("Account is already closed");

        Apply(
            new AccountClosed
            {
                AccountId = Id,
                Reason = reason?.GetValueOrDefault("No reason provided") ?? "No reason provided",
                ClosedAt = DateTimeOffset.UtcNow,
            }
        );

        return new Success();
    }

    public void ChargeOverdraftFee(Money feeAmount)
    {
        Apply(
            new OverdraftFeeCharged
            {
                AccountId = Id,
                FeeAmount = feeAmount,
                ChargedAt = DateTimeOffset.UtcNow,
            }
        );
    }

    public void AccrueInterest(Money interestAmount)
    {
        Apply(
            new InterestAccrued
            {
                AccountId = Id,
                InterestAmount = interestAmount,
                AccruedAt = DateTimeOffset.UtcNow,
            }
        );
    }

    // Event handlers
    protected override void When(BaseEvent @event)
    {
        switch (@event)
        {
            case AccountOpened e:
                Id = e.AccountId;
                CustomerId = e.CustomerId;
                Balance = e.InitialBalance;
                Status = AccountStatus.Active;
                CreatedAt = e.OpenedAt;
                break;

            case MoneyDeposited e:
                Balance += e.Amount;
                LastTransactionAt = e.DepositedAt;
                break;

            case MoneyWithdrawn e:
                Balance -= e.Amount;
                LastTransactionAt = e.WithdrawnAt;
                break;

            case MoneyTransferred e when e.FromAccountId == Id:
                Balance -= e.Amount;
                LastTransactionAt = e.TransferredAt;
                break;

            case MoneyTransferred e when e.ToAccountId == Id:
                Balance += e.Amount;
                LastTransactionAt = e.TransferredAt;
                break;

            case AccountFrozen:
                Status = AccountStatus.Frozen;
                break;

            case AccountUnfrozen:
                Status = AccountStatus.Active;
                break;

            case AccountClosed:
                Status = AccountStatus.Closed;
                break;

            case OverdraftFeeCharged e:
                Balance -= e.FeeAmount;
                LastTransactionAt = e.ChargedAt;
                break;

            case InterestAccrued e:
                Balance += e.InterestAmount;
                LastTransactionAt = e.AccruedAt;
                break;
        }
    }

    protected override void EnsureValidState()
    {
        if (Id == null)
            throw new InvalidOperationException("Account must have an ID");

        if (CustomerId == null)
            throw new InvalidOperationException("Account must have a customer ID");

        if (Balance.Amount < 0 && Status == AccountStatus.Active)
            throw new InvalidOperationException("Active account cannot have negative balance");
    }
}

// Result types for functional programming
public record Success;

public record ValidationError(string Message);

public record InsufficientFunds(string Message);

public record AccountFrozenError(string Message);
