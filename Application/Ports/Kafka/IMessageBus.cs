using Domain.OsagoAggregate.DomainEvents;
using Domain.VehicleDocumentsAggregate.DomainEvents;

namespace Application.Ports.Kafka;

public interface IMessageBus
{
    Task Publish(OsagoExpiredDomainEvent domainEvent, CancellationToken cancellationToken);

    Task Publish(DocumentAddingCompletedDomainEvent domainEvent, CancellationToken cancellationToken);
}