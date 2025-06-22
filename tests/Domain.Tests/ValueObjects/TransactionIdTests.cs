using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public class TransactionIdTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithValidValue_ShouldCreateTransactionId()
    {
        // Arrange
        var value = "TXN123456789ABC";

        // Act
        var transactionId = new TransactionId(value);

        // Assert
        transactionId.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [Trait("Category", "Unit")]
    public void Constructor_WithInvalidValue_ShouldThrowArgumentException(string invalidValue)
    {
        // Act & Assert
        Action act = () => new TransactionId(invalidValue);
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Transaction ID cannot be null or empty*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Generate_ShouldCreateValidTransactionId()
    {
        // Act
        var transactionId = TransactionId.Generate();

        // Assert
        transactionId.Value.Should().StartWith("TXN");
        transactionId.Value.Length.Should().Be(15);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Generate_MultipleCalls_ShouldCreateUniqueIds()
    {
        // Act
        var id1 = TransactionId.Generate();
        var id2 = TransactionId.Generate();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var transactionId = new TransactionId("TXN123456789ABC");

        // Act
        string result = transactionId;

        // Assert
        result.Should().Be("TXN123456789ABC");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var transactionId = new TransactionId("TXN123456789ABC");

        // Act
        var result = transactionId.ToString();

        // Assert
        result.Should().Be("TXN123456789ABC");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var id1 = new TransactionId("TXN123456789ABC");
        var id2 = new TransactionId("TXN123456789ABC");

        // Act & Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
        (id1 != id2).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = new TransactionId("TXN123456789ABC");
        var id2 = new TransactionId("TXNABCDEF123456");

        // Act & Assert
        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetHashCode_SameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var id1 = new TransactionId("TXN123456789ABC");
        var id2 = new TransactionId("TXN123456789ABC");

        // Act & Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }
}
