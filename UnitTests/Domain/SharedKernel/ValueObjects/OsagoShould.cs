using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.Exceptions.DomainRulesViolationException;
using Domain.SharedKernel.ValueObjects;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.SharedKernel.ValueObjects;

[TestSubject(typeof(Osago))]
public class OsagoShould
{
    private readonly string _photoStorageBucketAndKey = "photoStorageBucketAndKey";
    private readonly DateOnly _dateOfIssue = DateOnly.FromDateTime(DateTime.Now);
    private readonly DateOnly _dateOfExpiry = DateOnly.FromDateTime(DateTime.Now.AddYears(1));
    
    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = Osago.Create(_photoStorageBucketAndKey, _dateOfIssue, _dateOfExpiry);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(_photoStorageBucketAndKey, actual.PhotoStorageBucketAndKey);
        Assert.Equal(_dateOfIssue, actual.DateOfIssue);
        Assert.Equal(_dateOfExpiry, actual.DateOfExpiry);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowValueIsRequiredExceptionIfPhotoStorageBucketAndKeyIsNullOrEmpty(string nullOrEmpty)
    {
        // Arrange

        // Act
void Act() => Osago.Create(nullOrEmpty, _dateOfIssue, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfDateOfIssueIsDefault()
    {
        // Arrange

        // Act
void Act() => Osago.Create(_photoStorageBucketAndKey, default, _dateOfExpiry);

        // Assert
        Assert.Throws<ValueOutOfRangeException>(Act);
    }

    [Fact]
    public void ThrowValueOutOfRangeExceptionIfDateOfExpiryIsDefault()
    {
        // Arrange

        // Act
        void Act() => Osago.Create(_photoStorageBucketAndKey, _dateOfIssue, default);

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
        void Act() => Osago.Create(_photoStorageBucketAndKey, dateOfIssue, dateOfExpiry);

        // Assert
        Assert.Throws<DomainRulesViolationException>(Act);
    }
}