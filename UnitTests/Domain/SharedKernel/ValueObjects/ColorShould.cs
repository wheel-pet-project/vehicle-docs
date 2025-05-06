using Domain.SharedKernel.Exceptions.PublicException;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(Color))]
public class ColorShould
{
    [Fact]
    public void FromNameReturnCorrectColor()
    {
        // Arrange
        var redColorName = Color.Red.ToString();

        // Act
        var actual = Color.FromName(redColorName);

        // Assert
        Assert.Equal(actual.ToString(), redColorName);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfColorIsUnknown()
    {
        // Arrange
        var coralСolorName = "Coral";

        // Act
        void Act()
        {
            Color.FromName(coralСolorName);
        }

        // Assert
        Assert.Throws<ValueIsUnsupportedException>(Act);
    }

    [Fact]
    public void EqualOperatorReturnTrueForEqualColors()
    {
        // Arrange
        var color1 = Color.Red;
        var color2 = Color.Red;

        // Act
        var actual = color1 == color2;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void NotEqualOperatorReturnTrueForDifferentColors()
    {
        // Arrange
        var color1 = Color.Red;
        var color2 = Color.Blue;

        // Act
        var actual = color1 != color2;

        // Assert
        Assert.True(actual);
    }
}