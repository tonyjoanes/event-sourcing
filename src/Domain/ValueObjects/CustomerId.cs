namespace Domain.ValueObjects;

public record CustomerId
{
    public string Value { get; }

    public CustomerId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Customer ID cannot be null or empty", nameof(value));

        Value = value;
    }

    public static CustomerId Generate()
    {
        return new CustomerId($"CUST{Guid.NewGuid():N}".Substring(0, 8).ToUpper());
    }

    public static implicit operator string(CustomerId customerId) => customerId.Value;

    public override string ToString() => Value;
} 