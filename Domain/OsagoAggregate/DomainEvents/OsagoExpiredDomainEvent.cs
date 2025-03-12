using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.OsagoAggregate.DomainEvents;

public record OsagoExpiredDomainEvent : DomainEvent
{
    public OsagoExpiredDomainEvent(Guid vehicleDocumentsId)
    {
        if (vehicleDocumentsId == Guid.Empty)
            throw new ValueIsRequiredException($"{nameof(vehicleDocumentsId)} cannot be empty.");

        VehicleDocumentsId = vehicleDocumentsId;
    }

    public Guid VehicleDocumentsId { get; }
}