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
    [InlineData(1, 1, 0)]
    [InlineData(0, 1, 1)]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 1)]
    public void AddingCompletedReturnFalseIfOneOfDocumentsNotAdded(int isPtsAdded, int isStsAdded, int isOsagoAdded)
    {
        // Arrange
        var status = Status.Create();
        if (isPtsAdded == 1) status.MarkAsPtsAdded();
        if (isStsAdded == 1) status.MarkAsStsAdded();
        if (isOsagoAdded == 1) status.MarkAsOsagoAdded();

        // Act
        var actual = status.AddingCompleted;

        // Assert
        Assert.False(actual);
    }
}