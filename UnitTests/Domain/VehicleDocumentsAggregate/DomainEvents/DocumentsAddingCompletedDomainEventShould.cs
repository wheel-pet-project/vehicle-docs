using Domain.SharedKernel.Exceptions.ArgumentException;
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
    public void ThrowValueIsRequiredExceptionIfVehicleIdIsEmpty()
    {
        // Arrange

        // Act
        void Act()
        {
            new DocumentAddingCompletedDomainEvent(_sagaId, Guid.Empty);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
    
    [Fact]
    public void ThrowValueIsRequiredExceptionIfSagaIdIsEmpty()
    {
        // Arrange

        // Act
        void Act()
        {
            new DocumentAddingCompletedDomainEvent(Guid.Empty, _vehicleId);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}