using Domain.Services;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Services;

public class InterestCalculatorTests
{
    private readonly InterestCalculator _calculator;

    public InterestCalculatorTests()
    {
        _calculator = new InterestCalculator();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateDailyInterest_WithPositivePrincipal_ShouldCalculateCorrectly()
    {
        // Arrange
        var principal = new Money(10000m, "USD");
        var rate = new InterestRate(18.25m, InterestRateType.Annual); // 18.25% APR

        // Act
        var interest = _calculator.CalculateDailyInterest(principal, rate);

        // Assert
        interest.Amount.Should().BeApproximately(5.0m, 0.1m); // 10000 * (18.25/365/100) ≈ 5.0
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateDailyInterest_WithZeroPrincipal_ShouldReturnZero()
    {
        // Arrange
        var principal = new Money(0m, "USD");
        var rate = new InterestRate(18.25m, InterestRateType.Annual);

        // Act
        var interest = _calculator.CalculateDailyInterest(principal, rate);

        // Assert
        interest.Amount.Should().Be(0m);
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateDailyInterest_WithNegativePrincipal_ShouldReturnZero()
    {
        // Arrange
        var calculator = new InterestCalculator();
        var principal = Money.CreateAllowNegative(-1000m, "USD");
        var rate = new InterestRate(5.0m);

        // Act
        var result = calculator.CalculateDailyInterest(principal, rate);

        // Assert
        result.Should().Be(Money.Zero("USD"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateMonthlyInterest_WithPositivePrincipal_ShouldCalculateCorrectly()
    {
        // Arrange
        var principal = new Money(10000m, "USD");
        var rate = new InterestRate(12.0m, InterestRateType.Annual); // 12% APR

        // Act
        var interest = _calculator.CalculateMonthlyInterest(principal, rate);

        // Assert
        interest.Amount.Should().Be(100m); // 10000 * (12/12/100) = 100
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateMonthlyInterest_WithMonthlyRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var principal = new Money(10000m, "USD");
        var rate = new InterestRate(1.5m, InterestRateType.Monthly); // 1.5% per month

        // Act
        var interest = _calculator.CalculateMonthlyInterest(principal, rate);

        // Assert
        interest.Amount.Should().Be(150m); // 10000 * (1.5/100) = 150
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateAnnualInterest_WithPositivePrincipal_ShouldCalculateCorrectly()
    {
        // Arrange
        var principal = new Money(10000m, "USD");
        var rate = new InterestRate(5.5m, InterestRateType.Annual); // 5.5% APR

        // Act
        var interest = _calculator.CalculateAnnualInterest(principal, rate);

        // Assert
        interest.Amount.Should().Be(550m); // 10000 * (5.5/100) = 550
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateOverdraftInterest_WithNegativeAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var calculator = new InterestCalculator();
        var overdraftAmount = Money.CreateAllowNegative(-500m, "USD");
        var rate = new InterestRate(18.99m);
        var days = 30;

        // Act
        var result = calculator.CalculateOverdraftInterest(overdraftAmount, rate, days);

        // Assert
        result.Amount.Should().BeGreaterThan(0);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateOverdraftInterest_WithPositiveAmount_ShouldReturnZero()
    {
        // Arrange
        var amount = new Money(1000m, "USD");
        var rate = new InterestRate(18.25m, InterestRateType.Annual);
        var days = 30;

        // Act
        var interest = _calculator.CalculateOverdraftInterest(amount, rate, days);

        // Assert
        interest.Amount.Should().Be(0m);
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateCompoundInterest_WithAnnualCompounding_ShouldCalculateCorrectly()
    {
        // Arrange
        var principal = new Money(10000m, "USD");
        var rate = new InterestRate(5.0m, InterestRateType.Annual); // 5% APR
        var periods = 1; // 1 year

        // Act
        var interest = _calculator.CalculateCompoundInterest(principal, rate, periods);

        // Assert
        interest.Amount.Should().Be(500m); // 10000 * (1 + 0.05)^1 - 10000 = 500
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateCompoundInterest_WithMonthlyCompounding_ShouldCalculateCorrectly()
    {
        // Arrange
        var principal = new Money(10000m, "USD");
        var rate = new InterestRate(12.0m, InterestRateType.Annual); // 12% APR
        var periods = 1; // 1 year
        var timesPerPeriod = 12; // Monthly compounding

        // Act
        var interest = _calculator.CalculateCompoundInterest(
            principal,
            rate,
            periods,
            timesPerPeriod
        );

        // Assert
        interest.Amount.Should().BeApproximately(1268.25m, 0.01m); // Compound interest formula
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateCompoundInterest_WithZeroPrincipal_ShouldReturnZero()
    {
        // Arrange
        var principal = new Money(0m, "USD");
        var rate = new InterestRate(5.0m, InterestRateType.Annual);
        var periods = 1;

        // Act
        var interest = _calculator.CalculateCompoundInterest(principal, rate, periods);

        // Assert
        interest.Amount.Should().Be(0m);
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateSavingsInterest_WithValidPeriod_ShouldCalculateCorrectly()
    {
        // Arrange
        var balance = new Money(10000m, "USD");
        var rate = new InterestRate(3.65m, InterestRateType.Annual); // 3.65% APR
        var fromDate = DateTimeOffset.UtcNow.AddDays(-30);
        var toDate = DateTimeOffset.UtcNow;

        // Act
        var interest = _calculator.CalculateSavingsInterest(balance, rate, fromDate, toDate);

        // Assert
        interest.Amount.Should().BeApproximately(30m, 1m); // 10000 * (3.65/365/100) * 30 ≈ 30
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateSavingsInterest_WithZeroBalance_ShouldReturnZero()
    {
        // Arrange
        var balance = new Money(0m, "USD");
        var rate = new InterestRate(3.65m, InterestRateType.Annual);
        var fromDate = DateTimeOffset.UtcNow.AddDays(-30);
        var toDate = DateTimeOffset.UtcNow;

        // Act
        var interest = _calculator.CalculateSavingsInterest(balance, rate, fromDate, toDate);

        // Assert
        interest.Amount.Should().Be(0m);
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateSavingsInterest_WithInvalidPeriod_ShouldReturnZero()
    {
        // Arrange
        var balance = new Money(10000m, "USD");
        var rate = new InterestRate(3.65m, InterestRateType.Annual);
        var fromDate = DateTimeOffset.UtcNow;
        var toDate = DateTimeOffset.UtcNow.AddDays(-30); // Invalid: toDate before fromDate

        // Act
        var interest = _calculator.CalculateSavingsInterest(balance, rate, fromDate, toDate);

        // Assert
        interest.Amount.Should().Be(0m);
        interest.Currency.Should().Be("USD");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateSavingsInterest_WithSameDate_ShouldReturnZero()
    {
        // Arrange
        var balance = new Money(10000m, "USD");
        var rate = new InterestRate(3.65m, InterestRateType.Annual);
        var date = DateTimeOffset.UtcNow;

        // Act
        var interest = _calculator.CalculateSavingsInterest(balance, rate, date, date);

        // Assert
        interest.Amount.Should().Be(0m);
        interest.Currency.Should().Be("USD");
    }
}
