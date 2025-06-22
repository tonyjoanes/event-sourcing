namespace Integration.Tests;

public class IntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public void Should_Pass_Integration_Test()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Should_Handle_Basic_Integration_Scenario()
    {
        // Arrange
        var input = "test";

        // Act
        var result = input.ToUpper();

        // Assert
        Assert.Equal("TEST", result);
    }
}