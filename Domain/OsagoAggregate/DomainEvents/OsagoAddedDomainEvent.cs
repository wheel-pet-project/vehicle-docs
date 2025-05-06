using Domain.SharedKernel;

namespace Domain.OsagoAggregate.DomainEvents;

public record OsagoAddedDomainEvent : DomainEvent
{
    public OsagoAddedDomainEvent(Guid vehicleDocumentsId)
    {
        if (vehicleDocumentsId == Guid.Empty)
            throw new ArgumentException(
                $"{nameof(vehicleDocumentsId)} cannot be empty");

        VehicleDocumentsId = vehicleDocumentsId;
    }

    public Guid VehicleDocumentsId { get; }
}