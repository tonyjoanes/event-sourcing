using Infrastructure.EventStore;

namespace Infrastructure.Tests.EventStore;

public class ConcurrencyExceptionTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        var message = "Concurrency conflict detected";

        // Act
        var exception = new ConcurrencyException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithMessageAndInnerException_SetsBothProperties()
    {
        // Arrange
        var message = "Concurrency conflict detected";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new ConcurrencyException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNullMessage_HandlesGracefully()
    {
        // Act & Assert - Should not throw
        var exception = new ConcurrencyException(null!);
        Assert.NotNull(exception);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithEmptyMessage_SetsEmptyMessage()
    {
        // Arrange
        var message = string.Empty;

        // Act
        var exception = new ConcurrencyException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNullInnerException_HandlesGracefully()
    {
        // Arrange
        var message = "Test message";

        // Act
        var exception = new ConcurrencyException(message, null!);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.InnerException);
    }
}
