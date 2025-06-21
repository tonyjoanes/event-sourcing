using Domain.ValueObjects;

namespace Domain.Services;

public class InterestCalculator
{
    public Money CalculateDailyInterest(Money principal, InterestRate rate)
    {
        if (principal.Amount <= 0)
            return Money.Zero(principal.Currency);

        var dailyRate = rate.GetDailyRate() / 100; // Convert percentage to decimal
        var interest = principal.Amount * dailyRate;

        return new Money(interest, principal.Currency);
    }

    public Money CalculateMonthlyInterest(Money principal, InterestRate rate)
    {
        if (principal.Amount <= 0)
            return Money.Zero(principal.Currency);

        var monthlyRate = rate.GetMonthlyRate() / 100; // Convert percentage to decimal
        var interest = principal.Amount * monthlyRate;

        return new Money(interest, principal.Currency);
    }

    public Money CalculateAnnualInterest(Money principal, InterestRate rate)
    {
        if (principal.Amount <= 0)
            return Money.Zero(principal.Currency);

        var annualRate = rate.Value / 100; // Convert percentage to decimal
        var interest = principal.Amount * annualRate;

        return new Money(interest, principal.Currency);
    }

    public Money CalculateOverdraftInterest(Money overdraftAmount, InterestRate rate, int days)
    {
        if (overdraftAmount.Amount >= 0)
            return Money.Zero(overdraftAmount.Currency);

        var dailyRate = rate.GetDailyRate() / 100;
        var interest = Math.Abs(overdraftAmount.Amount) * dailyRate * days;

        return new Money(interest, overdraftAmount.Currency);
    }

    public Money CalculateCompoundInterest(
        Money principal,
        InterestRate rate,
        int periods,
        int timesPerPeriod = 1
    )
    {
        if (principal.Amount <= 0)
            return Money.Zero(principal.Currency);

        var ratePerPeriod = rate.Value / 100m / timesPerPeriod;
        var compoundFactor = (decimal)Math.Pow((double)(1 + ratePerPeriod), periods * timesPerPeriod);
        var finalAmount = principal.Amount * compoundFactor;
        var interest = finalAmount - principal.Amount;

        return new Money(interest, principal.Currency);
    }

    public Money CalculateSavingsInterest(
        Money balance,
        InterestRate rate,
        DateTimeOffset fromDate,
        DateTimeOffset toDate
    )
    {
        if (balance.Amount <= 0)
            return Money.Zero(balance.Currency);

        var days = (toDate - fromDate).Days;
        if (days <= 0)
            return Money.Zero(balance.Currency);

        return CalculateDailyInterest(balance, rate) * days;
    }
}
