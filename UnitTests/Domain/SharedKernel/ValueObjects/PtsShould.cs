using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(PtsShould))]
public class PtsShould
{
    private readonly string _frontPhotoStorageBucketAndKey = "frontPhotoStorageBucketAndKey";
    private readonly string _backPhotoStorageBucketAndKey = "backPhotoStorageBucketAndKey";
    private readonly DateOnly _yearOfManufacture = DateOnly.FromDateTime(DateTime.Now.AddYears(-2));
    private readonly Color _color = Color.Black;
    private readonly Vin _vin = Vin.Create("SALYA2BN2KA791786");
    
    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = Pts.Create(_frontPhotoStorageBucketAndKey, _backPhotoStorageBucketAndKey, _yearOfManufacture,
            _color, _vin);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(_frontPhotoStorageBucketAndKey, actual.FrontPhotoStorageBucketAndKey);
        Assert.Equal(_backPhotoStorageBucketAndKey, actual.BackPhotoStorageBucketAndKey);
        Assert.Equal(_yearOfManufacture, actual.YearOfManufacture);
        Assert.Equal(_color, actual.Color);
        Assert.Equal(_vin, actual.Vin);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowValueIsRequiredExceptionIfFrontPhotoStorageBucketAndKeyIsNullOrEmpty(string nullOrEmpty)
    {
        // Arrange

        // Act
void Act() => Pts.Create(nullOrEmpty, _backPhotoStorageBucketAndKey, _yearOfManufacture, _color, _vin);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowValueIsRequiredExceptionIfBackPhotoStorageBucketAndKeyIsNullOrEmpty(string nullOrEmpty)
    {
        // Arrange

        // Act
        void Act() => Pts.Create(_frontPhotoStorageBucketAndKey, nullOrEmpty, _yearOfManufacture, _color, _vin);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfYearOfManufactureIsDefault()
    {
        // Arrange

        // Act
        void Act() => Pts.Create(_frontPhotoStorageBucketAndKey, _backPhotoStorageBucketAndKey, default, _color, _vin);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfColorIsNull()
    {
        // Arrange

        void Act() => Pts.Create(_frontPhotoStorageBucketAndKey, _backPhotoStorageBucketAndKey, _yearOfManufacture,
            null!, _vin);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfVinIsNull()
    {
        // Arrange

        // Act
        void Act() => Pts.Create(_frontPhotoStorageBucketAndKey, _backPhotoStorageBucketAndKey, _yearOfManufacture,
            _color, null!);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void EqualOperatorReturnTrueForEqualPts()
    {
        // Arrange
        var pts1 = Pts.Create(_frontPhotoStorageBucketAndKey, _backPhotoStorageBucketAndKey, _yearOfManufacture, _color,
            _vin);
        var pts2 = Pts.Create(_frontPhotoStorageBucketAndKey, _backPhotoStorageBucketAndKey, _yearOfManufacture, _color,
            _vin);

        // Act
        var actual = pts1 == pts2;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void NotEqualOperatorReturnTrueForNotEqualPts()
    {
        // Arrange
        var pts1 = Pts.Create("anotherkey", _backPhotoStorageBucketAndKey, _yearOfManufacture, _color,
            _vin);
        var pts2 = Pts.Create(_frontPhotoStorageBucketAndKey, _backPhotoStorageBucketAndKey, _yearOfManufacture, _color,
            _vin);

        // Act
        var actual = pts1 != pts2;

        // Assert
        Assert.True(actual);
    }
}