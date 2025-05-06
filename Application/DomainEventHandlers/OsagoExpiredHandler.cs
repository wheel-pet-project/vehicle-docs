using Application.Ports.Kafka;
using Domain.OsagoAggregate.DomainEvents;
using MediatR;

namespace Application.DomainEventHandlers;

public class OsagoExpiredHandler(IMessageBus messageBus) : INotificationHandler<OsagoExpiredDomainEvent>
{
    public async Task Handle(OsagoExpiredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await messageBus.Publish(domainEvent, cancellationToken);
    }
}