using System.Globalization;

namespace Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "USD") => new(0, currency);

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Money amount cannot be negative", nameof(amount));

        return new Money(amount, currency);
    }

    public static Money CreateAllowNegative(decimal amount, string currency)
    {
        return new Money(amount, currency);
    }

    public override string ToString()
    {
        return $"{Currency} {Amount:F2}";
    }

    // Arithmetic operators
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot add money with different currencies: {left.Currency} and {right.Currency}"
            );

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot subtract money with different currencies: {left.Currency} and {right.Currency}"
            );

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot multiply money with different currencies: {left.Currency} and {right.Currency}"
            );

        return new Money(left.Amount * right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }

    public static Money operator *(decimal multiplier, Money money)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }

    // Comparison operators
    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot compare money with different currencies: {left.Currency} and {right.Currency}"
            );

        return left.Amount < right.Amount;
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot compare money with different currencies: {left.Currency} and {right.Currency}"
            );

        return left.Amount > right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot compare money with different currencies: {left.Currency} and {right.Currency}"
            );

        return left.Amount <= right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot compare money with different currencies: {left.Currency} and {right.Currency}"
            );

        return left.Amount >= right.Amount;
    }
}
