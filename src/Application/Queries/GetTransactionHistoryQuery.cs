using Domain.ValueObjects;

namespace Application.Queries;

public record GetTransactionHistoryQuery
{
    public required AccountId AccountId { get; init; }
    public int? Limit { get; init; } = 50;
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
}
