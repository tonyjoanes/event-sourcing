namespace Infrastructure.ReadModels.Projections;

public class TransactionHistoryProjection
{
    public string Id { get; set; } = default!; // TransactionId
    public string AccountId { get; set; } = default!;
    public string Type { get; set; } = default!; // Deposit, Withdrawal, etc.
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
    public string? RelatedAccountId { get; set; } // For transfers
    public string CustomerId { get; set; } = default!;
} 