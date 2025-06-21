using Xunit;

namespace Infrastructure.Tests;

public class InfrastructureTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Should_Pass_Infrastructure_Test()
    {
        // Arrange
        var expected = true;
        
        // Act
        var actual = true;
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public void Should_Handle_Infrastructure_Scenario()
    {
        // Arrange
        var input = "infrastructure";
        
        // Act
        var result = input.Length;
        
        // Assert
        Assert.Equal(14, result);
    }
} 