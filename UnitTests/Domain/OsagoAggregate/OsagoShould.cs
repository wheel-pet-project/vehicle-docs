using Domain.OsagoAggregate;
using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using JetBrains.Annotations;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace UnitTests.Domain.OsagoAggregate;

[TestSubject(typeof(Osago))]
public class OsagoShould
{
    private readonly Guid _vehicleDocumentsId = Guid.NewGuid();
    private readonly string _photoStorageBucketAndKey = "photoStorageBucketAndKey";
    private readonly DateOnly _dateOfIssue = DateOnly.FromDateTime(DateTime.Now);
    private readonly DateOnly _dateOfExpiry = DateOnly.FromDateTime(DateTime.Now.AddYears(1));

    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = Osago.Create(_vehicleDocumentsId, _photoStorageBucketAndKey, _dateOfIssue, _dateOfExpiry);

        // Assert
        Assert.NotNull(actual);
        Assert.NotEqual(Guid.Empty, actual.Id);
        Assert.Equal(ExpiryStatus.NotExpired, actual.ExpiryStatus);
        Assert.Equal(_photoStorageBucketAndKey, actual.PhotoStorageBucketAndKey);
        Assert.Equal(_dateOfIssue, actual.DateOfIssue);
        Assert.Equal(_dateOfExpiry, actual.DateOfExpiry);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfVehicleDocumentsIdIsEmpty()
    {
        // Arrange

        // Act
        void Act()
        {
            Osago.Create(Guid.Empty, _photoStorageBucketAndKey, _dateOfIssue, _dateOfExpiry);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowValueIsRequiredExceptionIfPhotoStorageBucketAndKeyIsNullOrEmpty(string nullOrEmpty)
    {
        // Arrange

        // Act
        void Act()
        {
            Osago.Create(_vehicleDocumentsId, nullOrEmpty, _dateOfIssue, _dateOfExpiry);
        }

// Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfDateOfIssueIsDefault()
    {
        // Arrange

        // Act
        void Act()
        {
            Osago.Create(_vehicleDocumentsId, _photoStorageBucketAndKey, default, _dateOfExpiry);
        }

// Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfDateOfExpiryIsDefault()
    {
        // Arrange

        // Act
        void Act()
        {
            Osago.Create(_vehicleDocumentsId, _photoStorageBucketAndKey, _dateOfIssue, default);
        }

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void ThrowDomainRulesViolationExceptionIfDateOfIssueGreaterThanDateOfExpiry()
    {
        // Arrange
        var dateOfIssue = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var dateOfExpiry = DateOnly.FromDateTime(DateTime.Now);

        // Act
        void Act()
        {
            Osago.Create(_vehicleDocumentsId, _photoStorageBucketAndKey, dateOfIssue, dateOfExpiry);
        }

        // Assert
        Assert.Throws<DomainRulesViolationException>(Act);
    }

    [Fact]
    public void IsExpiredReturnFalseIfDateOfExpiryNotComeYet()
    {
        // Arrange
        var dateOfIssue = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
        var dateOfExpiry = DateOnly.FromDateTime(DateTime.Now);
        var osago = Osago.Create(_vehicleDocumentsId, _photoStorageBucketAndKey, dateOfIssue, dateOfExpiry);
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(-10));

        // Act
        var actual = osago.IsExpired(fakeTimeProvider);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsExpiredReturnTrueIfDateOfExpiryComeYet()
    {
        // Arrange
        var dateOfIssue = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
        var dateOfExpiry = DateOnly.FromDateTime(DateTime.Now);
        var osago = Osago.Create(_vehicleDocumentsId, _photoStorageBucketAndKey, dateOfIssue, dateOfExpiry);
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(+10));

        // Act
        var actual = osago.IsExpired(fakeTimeProvider);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void ExpireChangeExpiryStatus()
    {
        // Arrange
        var dateOfIssue = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
        var dateOfExpiry = DateOnly.FromDateTime(DateTime.Now);
        var osago = Osago.Create(_vehicleDocumentsId, _photoStorageBucketAndKey, dateOfIssue, dateOfExpiry);
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetUtcNow(new DateTimeOffset(DateTime.Now.AddDays(1)));

        // Act
        osago.Expire(fakeTimeProvider);

        // Assert
        Assert.Equal(ExpiryStatus.Expired, osago.ExpiryStatus);
    }

    [Fact]
    public void ThrowDomainRulesViolationExceptionIfExpireCallingIfDateOfExpiryNotComeYet()
    {
        // Arrange
        var dateOfIssue = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
        var dateOfExpiry = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var osago = Osago.Create(_vehicleDocumentsId, _photoStorageBucketAndKey, dateOfIssue, dateOfExpiry);
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        // Act
        void Act()
        {
            osago.Expire(fakeTimeProvider);
        }

        // Assert
        Assert.Throws<DomainRulesViolationException>(Act);
    }
}