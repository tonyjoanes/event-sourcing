using Domain.ValueObjects;

namespace Application.Commands;

public record DepositCommand
{
    public required AccountId AccountId { get; init; }
    public required Money Amount { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
} 