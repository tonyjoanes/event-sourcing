using Infrastructure.EventStore;
using Xunit;

namespace Infrastructure.Tests.EventStore;

public class RavenDbEventStoreValidationTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNullDocumentStore_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new RavenDbEventStore(null!));
        Assert.Equal("documentStore", exception.ParamName);
    }
} 