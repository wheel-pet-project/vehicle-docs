using Domain.VehicleDocumentsAggregate.DomainEvents;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.VehicleDocumentsAggregate.DomainEvents;

[TestSubject(typeof(DocumentAddingCompletedDomainEvent))]
public class DocumentsAddingCompletedDomainEventShould
{
    private readonly Guid _sagaId = Guid.NewGuid();
    private readonly Guid _vehicleId = Guid.NewGuid();

    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = new DocumentAddingCompletedDomainEvent(_sagaId, _vehicleId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(_vehicleId, actual.VehicleId);
    }

    [Fact]
    public void ThrowArgumentExceptionIfVehicleIdIsEmpty()
    {
        // Arrange

        // Act
        void Act()
        {
            new DocumentAddingCompletedDomainEvent(_sagaId, Guid.Empty);
        }

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }

    [Fact]
    public void ThrowArgumentExceptionIfSagaIdIsEmpty()
    {
        // Arrange

        // Act
        void Act()
        {
            new DocumentAddingCompletedDomainEvent(Guid.Empty, _vehicleId);
        }

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }
}