using System.Security.Cryptography;

namespace Domain.ValueObjects;

public record TransactionId
{
    public string Value { get; }

    public TransactionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Transaction ID cannot be null or empty", nameof(value));

        if (value.Length != 15)
            throw new ArgumentException("Transaction ID must be exactly 15 characters long", nameof(value));

        if (!value.StartsWith("TXN"))
            throw new ArgumentException("Transaction ID must start with 'TXN'", nameof(value));

        Value = value;
    }

    public static TransactionId Generate()
    {
        var randomBytes = new byte[6]; // 6 bytes = 12 hex characters
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        var hexString = Convert.ToHexString(randomBytes).ToUpperInvariant();
        var transactionId = $"TXN{hexString}";
        
        return new TransactionId(transactionId);
    }

    public override string ToString() => Value;

    public static implicit operator string(TransactionId transactionId) => transactionId.Value;
} 