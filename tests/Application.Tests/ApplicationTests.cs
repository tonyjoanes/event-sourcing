namespace Application.Tests;

public class ApplicationTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Should_Pass_Application_Test()
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
    public void Should_Handle_Application_Scenario()
    {
        // Arrange
        var input = "application";

        // Act
        var result = input.Contains("app");

        // Assert
        Assert.True(result);
    }
}
