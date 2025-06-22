using Domain.ValueObjects;

namespace Application.Commands;

public record TransferCommand
{
    public required AccountId FromAccountId { get; init; }
    public required AccountId ToAccountId { get; init; }
    public required Money Amount { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
}
