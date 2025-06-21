using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldSucceed()
    {
        // Act
        var money = new Money(100.50m);

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowException()
    {
        Action act = () => Money.Create(-10m, "USD");
        act.Should().Throw<ArgumentException>().WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Add_TwoMoneyObjects_ShouldReturnCorrectSum()
    {
        // Arrange
        var money1 = new Money(100m);
        var money2 = new Money(50m);

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrencies_ShouldThrowException()
    {
        // Arrange
        var usd = new Money(100m, "USD");
        var eur = new Money(50m, "EUR");

        // Act & Assert
        var action = () => usd + eur;
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add money with different currencies*");
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        // Act
        var zero = Money.Zero();

        // Assert
        zero.Amount.Should().Be(0m);
        zero.Currency.Should().Be("USD");
    }
} 