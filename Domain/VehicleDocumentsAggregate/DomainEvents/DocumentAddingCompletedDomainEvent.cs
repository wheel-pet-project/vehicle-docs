using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.VehicleDocumentsAggregate.DomainEvents;

public record DocumentAddingCompletedDomainEvent : DomainEvent
{
    public DocumentAddingCompletedDomainEvent(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(vehicleId)} cannot be empty");
        VehicleId = vehicleId;
    }
    
    public Guid VehicleId { get; }
}