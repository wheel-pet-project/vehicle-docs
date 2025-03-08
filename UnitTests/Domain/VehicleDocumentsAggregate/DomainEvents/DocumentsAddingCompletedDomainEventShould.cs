using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.VehicleDocumentsAggregate.DomainEvents;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.VehicleDocumentsAggregate.DomainEvents;

[TestSubject(typeof(DocumentAddingCompletedDomainEvent))]
public class DocumentsAddingCompletedDomainEventShould
{
    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();

        // Act
        var actual = new DocumentAddingCompletedDomainEvent(vehicleId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(vehicleId, actual.VehicleId);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfVehicleIdIsEmpty()
    {
        // Arrange
        var vehicleId = Guid.Empty; 

        // Act
        void Act() => new DocumentAddingCompletedDomainEvent(vehicleId);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}