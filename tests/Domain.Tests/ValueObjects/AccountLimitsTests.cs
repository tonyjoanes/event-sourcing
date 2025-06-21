using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public class AccountLimitsTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithValidParameters_ShouldCreateAccountLimits()
    {
        // Arrange
        var dailyLimit = new Money(1000m, "USD");
        var minBalance = new Money(100m, "USD");
        var overdraftLimit = new Money(500m, "USD");
        var overdraftRate = new InterestRate(18.99m);

        // Act
        var limits = new AccountLimits(dailyLimit, minBalance, overdraftLimit, overdraftRate);

        // Assert
        limits.DailyWithdrawalLimit.Should().Be(dailyLimit);
        limits.MinimumBalance.Should().Be(minBalance);
        limits.OverdraftLimit.Should().Be(overdraftLimit);
        limits.OverdraftRate.Should().Be(overdraftRate);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNegativeDailyLimit_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var negativeLimit = Money.CreateAllowNegative(-100m, "USD");
        var minimumBalance = new Money(100m, "USD");
        var overdraftLimit = new Money(500m, "USD");
        var overdraftRate = new InterestRate(18.99m);

        var action = () => new AccountLimits(negativeLimit, minimumBalance, overdraftLimit, overdraftRate);
        action.Should().Throw<ArgumentException>().WithMessage("*cannot be negative*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNegativeMinimumBalance_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var dailyLimit = new Money(1000m, "USD");
        var negativeMinimumBalance = Money.CreateAllowNegative(-50m, "USD");
        var overdraftLimit = new Money(500m, "USD");
        var overdraftRate = new InterestRate(18.99m);

        var action = () => new AccountLimits(dailyLimit, negativeMinimumBalance, overdraftLimit, overdraftRate);
        action.Should().Throw<ArgumentException>().WithMessage("*cannot be negative*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNegativeOverdraftLimit_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var dailyLimit = new Money(1000m, "USD");
        var minimumBalance = new Money(100m, "USD");
        var negativeOverdraftLimit = Money.CreateAllowNegative(-200m, "USD");
        var overdraftRate = new InterestRate(18.99m);

        var action = () => new AccountLimits(dailyLimit, minimumBalance, negativeOverdraftLimit, overdraftRate);
        action.Should().Throw<ArgumentException>().WithMessage("*cannot be negative*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithDifferentCurrencies_ShouldThrowArgumentException()
    {
        // Arrange
        var dailyLimit = new Money(1000m, "USD");
        var minBalance = new Money(100m, "EUR");
        var overdraftLimit = new Money(500m, "USD");
        var overdraftRate = new InterestRate(18.99m);

        // Act & Assert
        Action act = () => new AccountLimits(dailyLimit, minBalance, overdraftLimit, overdraftRate);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*All account limits must use the same currency*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Standard_ShouldCreateStandardLimits()
    {
        // Act
        var limits = AccountLimits.Standard();

        // Assert
        limits.DailyWithdrawalLimit.Should().Be(new Money(1000m, "USD"));
        limits.MinimumBalance.Should().Be(new Money(100m, "USD"));
        limits.OverdraftLimit.Should().Be(new Money(500m, "USD"));
        limits.OverdraftRate.Value.Should().Be(18.99m);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Standard_WithCurrency_ShouldCreateStandardLimitsWithCurrency()
    {
        // Act
        var limits = AccountLimits.Standard("EUR");

        // Assert
        limits.DailyWithdrawalLimit.Currency.Should().Be("EUR");
        limits.MinimumBalance.Currency.Should().Be("EUR");
        limits.OverdraftLimit.Currency.Should().Be("EUR");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Premium_ShouldCreatePremiumLimits()
    {
        // Act
        var limits = AccountLimits.Premium();

        // Assert
        limits.DailyWithdrawalLimit.Should().Be(new Money(5000m, "USD"));
        limits.MinimumBalance.Should().Be(new Money(1000m, "USD"));
        limits.OverdraftLimit.Should().Be(new Money(2000m, "USD"));
        limits.OverdraftRate.Value.Should().Be(15.99m);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Student_ShouldCreateStudentLimits()
    {
        // Act
        var limits = AccountLimits.Student();

        // Assert
        limits.DailyWithdrawalLimit.Should().Be(new Money(500m, "USD"));
        limits.MinimumBalance.Should().Be(new Money(0m, "USD"));
        limits.OverdraftLimit.Should().Be(new Money(0m, "USD"));
        limits.OverdraftRate.Value.Should().Be(0m);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsWithinDailyWithdrawalLimit_WithValidAmount_ShouldReturnTrue()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var amount = new Money(500m, "USD");

        // Act
        var result = limits.IsWithinDailyWithdrawalLimit(amount);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsWithinDailyWithdrawalLimit_WithExactLimit_ShouldReturnTrue()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var amount = new Money(1000m, "USD");

        // Act
        var result = limits.IsWithinDailyWithdrawalLimit(amount);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsWithinDailyWithdrawalLimit_WithExceedingAmount_ShouldReturnFalse()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var amount = new Money(1500m, "USD");

        // Act
        var result = limits.IsWithinDailyWithdrawalLimit(amount);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsAboveMinimumBalance_WithValidBalance_ShouldReturnTrue()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var balance = new Money(500m, "USD");

        // Act
        var result = limits.IsAboveMinimumBalance(balance);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsAboveMinimumBalance_WithExactMinimum_ShouldReturnTrue()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var balance = new Money(100m, "USD");

        // Act
        var result = limits.IsAboveMinimumBalance(balance);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsAboveMinimumBalance_WithBelowMinimum_ShouldReturnFalse()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var balance = new Money(50m, "USD");

        // Act
        var result = limits.IsAboveMinimumBalance(balance);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsWithinOverdraftLimit_WithPositiveBalance_ShouldReturnTrue()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var balance = new Money(500m, "USD");

        // Act
        var result = limits.IsWithinOverdraftLimit(balance);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsWithinOverdraftLimit_WithZeroBalance_ShouldReturnTrue()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var balance = new Money(0m, "USD");

        // Act
        var result = limits.IsWithinOverdraftLimit(balance);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsWithinOverdraftLimit_WithNegativeBalanceWithinLimit_ShouldReturnTrue()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var balance = Money.CreateAllowNegative(-200m, "USD");

        // Act
        var result = limits.IsWithinOverdraftLimit(balance);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsWithinOverdraftLimit_WithNegativeBalanceExceedingLimit_ShouldReturnFalse()
    {
        // Arrange
        var limits = AccountLimits.Standard();
        var balance = Money.CreateAllowNegative(-600m, "USD");

        // Act
        var result = limits.IsWithinOverdraftLimit(balance);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var limits = AccountLimits.Standard("USD");

        // Act
        var result = limits.ToString();

        // Assert
        result.Should().Contain("Daily Limit: USD 1000.00");
        result.Should().Contain("Min Balance: USD 100.00");
        result.Should().Contain("Overdraft: USD 500.00");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var limits1 = AccountLimits.Standard();
        var limits2 = AccountLimits.Standard();

        // Act & Assert
        limits1.Should().Be(limits2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Equality_WithDifferentLimits_ShouldNotBeEqual()
    {
        // Arrange
        var limits1 = AccountLimits.Standard();
        var limits2 = AccountLimits.Premium();

        // Act & Assert
        limits1.Should().NotBe(limits2);
    }
} 