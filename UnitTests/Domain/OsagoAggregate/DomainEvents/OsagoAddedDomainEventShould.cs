using Domain.OsagoAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.ArgumentException;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.OsagoAggregate.DomainEvents;

[TestSubject(typeof(OsagoAddedDomainEvent))]
public class OsagoAddedDomainEventShould
{
    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange
        var vehicleDocumentsId = Guid.NewGuid();

        // Act
        var actual = new OsagoAddedDomainEvent(vehicleDocumentsId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(vehicleDocumentsId, actual.VehicleDocumentsId);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfVehicleIdIsEmpty()
    {
        // Arrange
        var vehicleDocumentsId = Guid.Empty;

        // Act
        void Act()
        {
            new OsagoAddedDomainEvent(vehicleDocumentsId);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}