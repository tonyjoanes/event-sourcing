using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public class InterestRateTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithValidValue_ShouldCreateInterestRate()
    {
        // Arrange
        var value = 5.5m;

        // Act
        var rate = new InterestRate(value);

        // Assert
        rate.Value.Should().Be(value);
        rate.Type.Should().Be(InterestRateType.Annual);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithValidValueAndType_ShouldCreateInterestRate()
    {
        // Arrange
        var value = 5.5m;
        var type = InterestRateType.Monthly;

        // Act
        var rate = new InterestRate(value, type);

        // Assert
        rate.Value.Should().Be(value);
        rate.Type.Should().Be(type);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-10.5)]
    [InlineData(-100.0)]
    [Trait("Category", "Unit")]
    public void Constructor_WithNegativeValue_ShouldThrowArgumentException(decimal negativeValue)
    {
        // Act & Assert
        Action act = () => new InterestRate(negativeValue);
        act.Should().Throw<ArgumentException>().WithMessage("*Interest rate cannot be negative*");
    }

    [Theory]
    [InlineData(101.0)]
    [InlineData(150.5)]
    [InlineData(1000.0)]
    [Trait("Category", "Unit")]
    public void Constructor_WithValueOver100_ShouldThrowArgumentException(decimal over100Value)
    {
        // Act & Assert
        Action act = () => new InterestRate(over100Value);
        act.Should().Throw<ArgumentException>().WithMessage("*Interest rate cannot exceed 100%*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Zero_ShouldCreateZeroRate()
    {
        // Act
        var rate = InterestRate.Zero();

        // Assert
        rate.Value.Should().Be(0);
        rate.Type.Should().Be(InterestRateType.Annual);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Zero_WithType_ShouldCreateZeroRateWithType()
    {
        // Act
        var rate = InterestRate.Zero(InterestRateType.Monthly);

        // Assert
        rate.Value.Should().Be(0);
        rate.Type.Should().Be(InterestRateType.Monthly);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetDailyRate_AnnualRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var annualRate = new InterestRate(18.25m, InterestRateType.Annual); // 18.25% APR

        // Act
        var dailyRate = annualRate.GetDailyRate();

        // Assert
        dailyRate.Should().BeApproximately(0.05m, 0.01m); // 18.25 / 365 ≈ 0.05
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetDailyRate_MonthlyRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var monthlyRate = new InterestRate(1.5m, InterestRateType.Monthly); // 1.5% per month

        // Act
        var dailyRate = monthlyRate.GetDailyRate();

        // Assert
        dailyRate.Should().BeApproximately(0.05m, 0.01m); // 1.5 / 30 ≈ 0.05
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetDailyRate_DailyRate_ShouldReturnSameValue()
    {
        // Arrange
        var dailyRate = new InterestRate(0.05m, InterestRateType.Daily); // 0.05% per day

        // Act
        var result = dailyRate.GetDailyRate();

        // Assert
        result.Should().Be(0.05m);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetMonthlyRate_AnnualRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var annualRate = new InterestRate(12.0m, InterestRateType.Annual); // 12% APR

        // Act
        var monthlyRate = annualRate.GetMonthlyRate();

        // Assert
        monthlyRate.Should().Be(1.0m); // 12 / 12 = 1
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetMonthlyRate_MonthlyRate_ShouldReturnSameValue()
    {
        // Arrange
        var monthlyRate = new InterestRate(1.5m, InterestRateType.Monthly); // 1.5% per month

        // Act
        var result = monthlyRate.GetMonthlyRate();

        // Assert
        result.Should().Be(1.5m);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetMonthlyRate_DailyRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var dailyRate = new InterestRate(0.05m, InterestRateType.Daily); // 0.05% per day

        // Act
        var monthlyRate = dailyRate.GetMonthlyRate();

        // Assert
        monthlyRate.Should().Be(1.5m); // 0.05 * 30 = 1.5
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Addition_SameType_ShouldAddRates()
    {
        // Arrange
        var rate1 = new InterestRate(5.0m, InterestRateType.Annual);
        var rate2 = new InterestRate(3.0m, InterestRateType.Annual);

        // Act
        var result = rate1 + rate2;

        // Assert
        result.Value.Should().Be(8.0m);
        result.Type.Should().Be(InterestRateType.Annual);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Addition_DifferentTypes_ShouldThrowException()
    {
        // Arrange
        var rate1 = new InterestRate(5.0m, InterestRateType.Annual);
        var rate2 = new InterestRate(3.0m, InterestRateType.Monthly);

        // Act & Assert
        Action act = () => _ = rate1 + rate2;
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Cannot add interest rates with different types*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Subtraction_SameType_ShouldSubtractRates()
    {
        // Arrange
        var rate1 = new InterestRate(8.0m, InterestRateType.Annual);
        var rate2 = new InterestRate(3.0m, InterestRateType.Annual);

        // Act
        var result = rate1 - rate2;

        // Assert
        result.Value.Should().Be(5.0m);
        result.Type.Should().Be(InterestRateType.Annual);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Subtraction_DifferentTypes_ShouldThrowException()
    {
        // Arrange
        var rate1 = new InterestRate(8.0m, InterestRateType.Annual);
        var rate2 = new InterestRate(3.0m, InterestRateType.Monthly);

        // Act & Assert
        Action act = () => _ = rate1 - rate2;
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Cannot subtract interest rates with different types*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var rate = new InterestRate(5.5m, InterestRateType.Annual);

        // Act
        var result = rate.ToString();

        // Assert
        result.Should().Be("5.5000% (Annual)");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Equality_WithSameValueAndType_ShouldBeEqual()
    {
        // Arrange
        var rate1 = new InterestRate(5.5m, InterestRateType.Annual);
        var rate2 = new InterestRate(5.5m, InterestRateType.Annual);

        // Act & Assert
        rate1.Should().Be(rate2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var rate1 = new InterestRate(5.5m, InterestRateType.Annual);
        var rate2 = new InterestRate(6.0m, InterestRateType.Annual);

        // Act & Assert
        rate1.Should().NotBe(rate2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Equality_WithDifferentType_ShouldNotBeEqual()
    {
        // Arrange
        var rate1 = new InterestRate(5.5m, InterestRateType.Annual);
        var rate2 = new InterestRate(5.5m, InterestRateType.Monthly);

        // Act & Assert
        rate1.Should().NotBe(rate2);
    }
}
