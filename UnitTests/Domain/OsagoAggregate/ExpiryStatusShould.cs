using Domain.OsagoAggregate;
using Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.OsagoAggregate;

[TestSubject(typeof(ExpiryStatus))]
public class ExpiryExpiryShould
{
    [Fact]
    public void ReturnRightExpiryStatusFromName()
    {
        // Arrange
        var notExpired = ExpiryStatus.NotExpired;

        // Act
        var actual = ExpiryStatus.FromName(notExpired.Name);

        // Assert
        Assert.Equal(ExpiryStatus.NotExpired, actual);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfExpiryStatusNameIsUnknown()
    {
        // Arrange
        var invalidName = "unsupportedName";

        // Act
        void Act()
        {
            ExpiryStatus.FromName(invalidName);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void ReturnRightExpiryStatusFromId()
    {
        // Arrange
        var notExpiredStatusId = ExpiryStatus.NotExpired.Id;

        // Act
        var actual = ExpiryStatus.FromId(notExpiredStatusId);

        // Assert
        Assert.Equal(ExpiryStatus.NotExpired, actual);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfExpiryStatusIdIsUnknown()
    {
        // Arrange
        var invalidId = 111;

        // Act
        void Act()
        {
            ExpiryStatus.FromId(invalidId);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }
}