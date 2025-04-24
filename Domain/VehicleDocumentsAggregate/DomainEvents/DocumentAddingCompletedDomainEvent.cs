using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.VehicleDocumentsAggregate.DomainEvents;

public record DocumentAddingCompletedDomainEvent : DomainEvent
{
    public DocumentAddingCompletedDomainEvent(Guid sagaId, Guid vehicleId)
    {
        if (vehicleId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(vehicleId)} cannot be empty");
        if (sagaId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(sagaId)} cannot be empty");
        
        SagaId = sagaId;
        VehicleId = vehicleId;
    }

    public Guid SagaId { get; }
    public Guid VehicleId { get; }
}