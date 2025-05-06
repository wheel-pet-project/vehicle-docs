using Domain.OsagoAggregate.DomainEvents;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.OsagoAggregate.DomainEvents;

[TestSubject(typeof(OsagoExpiredDomainEvent))]
public class OsagoExpiredDomainEventShould
{
    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange
        var vehicleDocumentsId = Guid.NewGuid();

        // Act
        var actual = new OsagoExpiredDomainEvent(vehicleDocumentsId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(vehicleDocumentsId, actual.VehicleDocumentsId);
    }

    [Fact]
    public void ThrowArgumentExceptionIfVehicleIdIsEmpty()
    {
        // Arrange
        var vehicleDocumentsId = Guid.Empty;

        // Act
        void Act()
        {
            new OsagoExpiredDomainEvent(vehicleDocumentsId);
        }

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }
}