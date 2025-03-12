using Domain.VehicleDocumentsAggregate;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.VehicleDocumentsAggregate;

[TestSubject(typeof(Status))]
public class StatusShould
{
    [Fact]
    public void MarkAsPtsAddedChangeStatus()
    {
        // Arrange
        var status = Status.Create();

        // Act
        status.MarkAsPtsAdded();

        // Assert
        Assert.True(status.IsPtsAdded);
    }

    [Fact]
    public void MarkAsStsAddedChangeStatus()
    {
        // Arrange
        var status = Status.Create();

        // Act
        status.MarkAsStsAdded();

        // Assert
        Assert.True(status.IsStsAdded);
    }

    [Fact]
    public void MarkAsOsagoAddedChangeStatus()
    {
        // Arrange
        var status = Status.Create();

        // Act
        status.MarkAsOsagoAdded();

        // Assert
        Assert.True(status.IsOsagoAdded);
    }

    [Fact]
    public void AddingCompletedReturnTrueIfAllDocumentsAdded()
    {
        // Arrange
        var status = Status.Create();
        status.MarkAsPtsAdded();
        status.MarkAsStsAdded();
        status.MarkAsOsagoAdded();

        // Act
        var actual = status.AddingCompleted;

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    public void AddingCompletedReturnFalseIfOneOfDocumentsNotAdded(bool isPtsAdded, bool isStsAdded, bool isOsagoAdded)
    {
        // Arrange
        var status = Status.Create();
        if (isPtsAdded) status.MarkAsPtsAdded();
        if (isStsAdded) status.MarkAsStsAdded();
        if (isOsagoAdded) status.MarkAsOsagoAdded();

        // Act
        var actual = status.AddingCompleted;

        // Assert
        Assert.False(actual);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    public void FromValuesReturnCorrectStatus(bool isPtsAdded, bool isStsAdded, bool isOsagoAdded)
    {
        // Arrange

        // Act
        var actual = Status.FromValues(isPtsAdded, isStsAdded, isOsagoAdded);

        // Assert
        Assert.Equal(isPtsAdded, actual.IsPtsAdded);
        Assert.Equal(isStsAdded, actual.IsStsAdded);
        Assert.Equal(isOsagoAdded, actual.IsOsagoAdded);
    }
}