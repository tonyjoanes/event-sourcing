using Domain.ValueObjects;

namespace Application.Queries;

public record GetAccountSummaryQuery
{
    public required AccountId AccountId { get; init; }
}
