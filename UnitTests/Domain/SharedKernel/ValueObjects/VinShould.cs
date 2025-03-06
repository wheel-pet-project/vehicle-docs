using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.SharedKernel.ValueObjects;


[TestSubject(typeof(Vin))]
public class VinShould
{
    [Fact]
    public void CreateNewInstanceWithCorrectNumber()
    {
        // Arrange
        var input = "SALYA2BN2KA791786";

        // Act
        var actual = Vin.Create(input);

        // Assert
        Assert.Equal(input, actual.Number);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ThrowValueIsRequiredExceptionIfNumberIsNullOrEmpty(string? invalidNumber)
    {
        // Arrange

        // Act
        void Act()
        {
            Vin.Create(invalidNumber!);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void TrimInputNumber()
    {
        // Arrange
        var input = "  SALYA2BN2KA791786  ";

        // Act
        var actual = Vin.Create(input);

        // Assert
        Assert.Equal(input.Trim(), actual.Number);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfNumberLengthIsNot17()
    {
        // Arrange
        var numberWithInvalidLength = "1DW21OPD9VKM";

        // Act
        void Act()
        {
            Vin.Create(numberWithInvalidLength);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void ChangeCharactersToUpperCase()
    {
        // Arrange
        var input = "salya2BN2ka791786";

        // Act
        var actual = Vin.Create(input);

        // Assert
        Assert.Equal(input.ToUpper(), actual.Number);
    }

    [Fact]
    public void EqualOperatorReturnTrueForEqualNumbers()
    {
        // Arrange
        var input = "SALYA2BN2KA791786";
        var vin1 = Vin.Create(input);
        var vin2 = Vin.Create(input);

        // Act
        var actual = vin1 == vin2;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void NotEqualOperatorReturnTrueForNotEqualNumbers()
    {
        // Arrange
        var vin1 = Vin.Create("SALYA2BN2KA791786");
        var vin2 = Vin.Create("NMTDG26RX0R023893");

        // Act
        var actual = vin1 != vin2;

        // Assert
        Assert.True(actual);
    }
}