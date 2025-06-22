using Infrastructure.EventStore;

namespace Infrastructure.Tests.EventStore;

public class EventDocumentTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_CreatesInstanceWithDefaultValues()
    {
        // Act
        var eventDocument = new EventDocument();

        // Assert
        Assert.Equal(string.Empty, eventDocument.Id);
        Assert.Equal(string.Empty, eventDocument.AggregateId);
        Assert.Equal(0, eventDocument.Version);
        Assert.Equal(string.Empty, eventDocument.EventType);
        Assert.Equal(string.Empty, eventDocument.EventData);
        Assert.Equal(default(DateTimeOffset), eventDocument.Timestamp);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var eventDocument = new EventDocument();
        var id = "test-aggregate/1";
        var aggregateId = "test-aggregate";
        var version = 1L;
        var eventType = "TestEvent";
        var eventData = "{\"data\": \"test\"}";
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        eventDocument.Id = id;
        eventDocument.AggregateId = aggregateId;
        eventDocument.Version = version;
        eventDocument.EventType = eventType;
        eventDocument.EventData = eventData;
        eventDocument.Timestamp = timestamp;

        // Assert
        Assert.Equal(id, eventDocument.Id);
        Assert.Equal(aggregateId, eventDocument.AggregateId);
        Assert.Equal(version, eventDocument.Version);
        Assert.Equal(eventType, eventDocument.EventType);
        Assert.Equal(eventData, eventDocument.EventData);
        Assert.Equal(timestamp, eventDocument.Timestamp);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Properties_CanHandleNullValues()
    {
        // Arrange
        var eventDocument = new EventDocument();

        // Act & Assert - Should not throw
        eventDocument.Id = null!;
        eventDocument.AggregateId = null!;
        eventDocument.EventType = null!;
        eventDocument.EventData = null!;

        Assert.Null(eventDocument.Id);
        Assert.Null(eventDocument.AggregateId);
        Assert.Null(eventDocument.EventType);
        Assert.Null(eventDocument.EventData);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Properties_CanHandleEmptyStrings()
    {
        // Arrange
        var eventDocument = new EventDocument();

        // Act
        eventDocument.Id = string.Empty;
        eventDocument.AggregateId = string.Empty;
        eventDocument.EventType = string.Empty;
        eventDocument.EventData = string.Empty;

        // Assert
        Assert.Equal(string.Empty, eventDocument.Id);
        Assert.Equal(string.Empty, eventDocument.AggregateId);
        Assert.Equal(string.Empty, eventDocument.EventType);
        Assert.Equal(string.Empty, eventDocument.EventData);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Version_CanHandleNegativeValues()
    {
        // Arrange
        var eventDocument = new EventDocument();

        // Act
        eventDocument.Version = -1L;

        // Assert
        Assert.Equal(-1L, eventDocument.Version);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Version_CanHandleLargeValues()
    {
        // Arrange
        var eventDocument = new EventDocument();

        // Act
        eventDocument.Version = long.MaxValue;

        // Assert
        Assert.Equal(long.MaxValue, eventDocument.Version);
    }
}
