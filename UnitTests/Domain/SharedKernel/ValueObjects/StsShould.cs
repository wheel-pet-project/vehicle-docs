using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(Sts))]
public class StsShould
{
    private readonly string _frontPhotoStorageBucketAndKey = "frontPhotoStorageBucketAndKey";
    private readonly string _backPhotoStorageBucketAndKey = "backPhotoStorageBucketAndKey";

    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = Sts.Create(_frontPhotoStorageBucketAndKey, _backPhotoStorageBucketAndKey);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(_frontPhotoStorageBucketAndKey, actual.FrontPhotoStorageBucketAndKey);
        Assert.Equal(_backPhotoStorageBucketAndKey, actual.BackPhotoStorageBucketAndKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowValueIsRequiredExceptionIfFrontPhotoStorageBucketAndKeyIsNullOrEmpty(string nullOrEmpty)
    {
        // Arrange

        // Act
        void Act()
        {
            Sts.Create(nullOrEmpty, _backPhotoStorageBucketAndKey);
        }

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
        void Act()
        {
            Sts.Create(_frontPhotoStorageBucketAndKey, nullOrEmpty);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}