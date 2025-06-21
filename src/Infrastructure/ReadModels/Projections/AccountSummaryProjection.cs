namespace Infrastructure.ReadModels.Projections;

public class AccountSummaryProjection
{
    public string Id { get; set; } = default!; // AccountId
    public string CustomerId { get; set; } = default!;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTimeOffset? LastTransactionAt { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal OverdraftLimit { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MinimumBalance { get; set; }
} 