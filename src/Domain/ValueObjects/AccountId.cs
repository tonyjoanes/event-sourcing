namespace Domain.ValueObjects;

public record AccountId
{
    public string Value { get; }

    public AccountId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account ID cannot be null or empty", nameof(value));

        Value = value;
    }

    public static AccountId Generate()
    {
        return new AccountId($"ACC{Guid.NewGuid():N}".Substring(0, 8).ToUpper());
    }

    public static implicit operator string(AccountId accountId) => accountId.Value;

    public override string ToString() => Value;
} 