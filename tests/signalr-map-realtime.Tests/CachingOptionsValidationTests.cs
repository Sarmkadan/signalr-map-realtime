using Xunit;
using SignalRMapRealtime.Configuration;

namespace SignalRMapRealtime.Tests;

public class CachingOptionsValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_WhenOptionsAreValid()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        var errors = CachingOptionsValidation.Validate(options);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReturnsListWithErrors_WhenOptionsAreInvalid()
    {
        // Arrange
        var options = new CachingOptions { DefaultDurationSeconds = 0 };

        // Act
        var errors = CachingOptionsValidation.Validate(options);

        // Assert
        Assert.Single(errors);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenOptionsAreValid()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        var isValid = CachingOptionsValidation.IsValid(options);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenOptionsAreInvalid()
    {
        // Arrange
        var options = new CachingOptions { DefaultDurationSeconds = 0 };

        // Act
        var isValid = CachingOptionsValidation.IsValid(options);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WhenOptionsAreInvalid()
    {
        // Arrange
        var options = new CachingOptions { DefaultDurationSeconds = 0 };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => CachingOptionsValidation.EnsureValid(options));
    }

    [Fact]
    public void Validate_NullOptions_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => CachingOptionsValidation.Validate(null));
    }
}
