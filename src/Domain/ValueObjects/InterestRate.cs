namespace Domain.ValueObjects;

public record InterestRate
{
    public decimal Value { get; }
    public InterestRateType Type { get; }

    public InterestRate(decimal value, InterestRateType type = InterestRateType.Annual)
    {
        if (value < 0)
            throw new ArgumentException("Interest rate cannot be negative", nameof(value));

        if (value > 100)
            throw new ArgumentException("Interest rate cannot exceed 100%", nameof(value));

        Value = value;
        Type = type;
    }

    public decimal GetDailyRate()
    {
        return Type switch
        {
            InterestRateType.Annual => Value / 365,
            InterestRateType.Monthly => Value / 30,
            InterestRateType.Daily => Value,
            _ => throw new InvalidOperationException($"Unsupported interest rate type: {Type}"),
        };
    }

    public decimal GetMonthlyRate()
    {
        return Type switch
        {
            InterestRateType.Annual => Value / 12,
            InterestRateType.Monthly => Value,
            InterestRateType.Daily => Value * 30,
            _ => throw new InvalidOperationException($"Unsupported interest rate type: {Type}"),
        };
    }

    public static InterestRate Zero(InterestRateType type = InterestRateType.Annual) =>
        new(0, type);

    public static InterestRate operator +(InterestRate left, InterestRate right)
    {
        if (left.Type != right.Type)
            throw new InvalidOperationException(
                $"Cannot add interest rates with different types: {left.Type} and {right.Type}"
            );

        return new InterestRate(left.Value + right.Value, left.Type);
    }

    public static InterestRate operator -(InterestRate left, InterestRate right)
    {
        if (left.Type != right.Type)
            throw new InvalidOperationException(
                $"Cannot subtract interest rates with different types: {left.Type} and {right.Type}"
            );

        return new InterestRate(left.Value - right.Value, left.Type);
    }

    public override string ToString()
    {
        return $"{Value:F4}% ({Type})";
    }
}

public enum InterestRateType
{
    Annual,
    Monthly,
    Daily,
}
