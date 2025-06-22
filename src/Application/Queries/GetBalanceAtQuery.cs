using Domain.ValueObjects;

namespace Application.Queries;

public record GetBalanceAtQuery
{
    public required AccountId AccountId { get; init; }
    public required DateTimeOffset AtDate { get; init; }
}
