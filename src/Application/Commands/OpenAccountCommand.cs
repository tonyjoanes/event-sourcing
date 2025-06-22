using Domain.ValueObjects;

namespace Application.Commands;

public record OpenAccountCommand
{
    public required CustomerId CustomerId { get; init; }
    public required Money InitialBalance { get; init; }
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
} 