using Domain.OsagoAggregate;
using Domain.SharedKernel.Exceptions.PublicExceptions;
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
        Assert.Throws<ValueIsUnsupportedException>(Act);
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
        Assert.Throws<ValueIsUnsupportedException>(Act);
    }

    [Fact]
    public void EqualOperatorReturnTrueForEqualStatuses()
    {
        // Arrange
        var status1 = ExpiryStatus.NotExpired;
        var status2 = ExpiryStatus.NotExpired;

        // Act
        var actual = status1 == status2;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void NotEqualOperatorReturnTrueForDifferentStatuses()
    {
        // Arrange
        var status1 = ExpiryStatus.NotExpired;
        var status2 = ExpiryStatus.Expired;

        // Act
        var actual = status1 != status2;

        // Assert
        Assert.True(actual);
    }
}