using Domain.SharedKernel;

namespace Domain.OsagoAggregate.DomainEvents;

public record OsagoExpiredDomainEvent : DomainEvent
{
    public OsagoExpiredDomainEvent(Guid vehicleDocumentsId)
    {
        if (vehicleDocumentsId == Guid.Empty)
            throw new ArgumentException(
                $"{nameof(vehicleDocumentsId)} cannot be empty");

        VehicleDocumentsId = vehicleDocumentsId;
    }

    public Guid VehicleDocumentsId { get; }
}