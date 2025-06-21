using Domain.ValueObjects;

namespace Domain.ValueObjects;

public record AccountLimits
{
    public Money DailyWithdrawalLimit { get; }
    public Money MinimumBalance { get; }
    public Money OverdraftLimit { get; }
    public InterestRate OverdraftRate { get; }

    public AccountLimits(
        Money dailyWithdrawalLimit,
        Money minimumBalance,
        Money overdraftLimit,
        InterestRate overdraftRate
    )
    {
        if (dailyWithdrawalLimit.Amount < 0)
            throw new ArgumentException("Daily withdrawal limit cannot be negative", nameof(dailyWithdrawalLimit));
        
        if (minimumBalance.Amount < 0)
            throw new ArgumentException("Minimum balance cannot be negative", nameof(minimumBalance));
        
        if (overdraftLimit.Amount < 0)
            throw new ArgumentException("Overdraft limit cannot be negative", nameof(overdraftLimit));

        // Ensure all limits use the same currency
        if (dailyWithdrawalLimit.Currency != minimumBalance.Currency ||
            dailyWithdrawalLimit.Currency != overdraftLimit.Currency)
        {
            throw new ArgumentException("All account limits must use the same currency");
        }

        DailyWithdrawalLimit = dailyWithdrawalLimit;
        MinimumBalance = minimumBalance;
        OverdraftLimit = overdraftLimit;
        OverdraftRate = overdraftRate;
    }

    public static AccountLimits Standard(string currency = "USD")
    {
        return new AccountLimits(
            dailyWithdrawalLimit: new Money(1000m, currency),
            minimumBalance: new Money(100m, currency),
            overdraftLimit: new Money(500m, currency),
            overdraftRate: new InterestRate(18.99m) // 18.99% APR
        );
    }

    public static AccountLimits Premium(string currency = "USD")
    {
        return new AccountLimits(
            dailyWithdrawalLimit: new Money(5000m, currency),
            minimumBalance: new Money(1000m, currency),
            overdraftLimit: new Money(2000m, currency),
            overdraftRate: new InterestRate(15.99m) // 15.99% APR
        );
    }

    public static AccountLimits Student(string currency = "USD")
    {
        return new AccountLimits(
            dailyWithdrawalLimit: new Money(500m, currency),
            minimumBalance: new Money(0m, currency),
            overdraftLimit: new Money(0m, currency),
            overdraftRate: new InterestRate(0m) // No overdraft for students
        );
    }

    public bool IsWithinDailyWithdrawalLimit(Money amount)
    {
        return amount <= DailyWithdrawalLimit;
    }

    public bool IsAboveMinimumBalance(Money balance)
    {
        return balance >= MinimumBalance;
    }

    public bool IsWithinOverdraftLimit(Money balance)
    {
        // If balance is positive, we're within overdraft limit
        if (balance.Amount >= 0)
            return true;
        
        // If balance is negative, check if the negative amount is within overdraft limit
        return Math.Abs(balance.Amount) <= OverdraftLimit.Amount;
    }

    public override string ToString()
    {
        return $"Daily Limit: {DailyWithdrawalLimit}, Min Balance: {MinimumBalance}, Overdraft: {OverdraftLimit}";
    }
} 