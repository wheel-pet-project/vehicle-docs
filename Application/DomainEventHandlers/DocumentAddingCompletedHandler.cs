using Application.Ports.Kafka;
using Domain.VehicleDocumentsAggregate.DomainEvents;
using MediatR;

namespace Application.DomainEventHandlers;

public class DocumentAddingCompletedHandler(IMessageBus messageBus)
    : INotificationHandler<DocumentAddingCompletedDomainEvent>
{
    public async Task Handle(DocumentAddingCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await messageBus.Publish(domainEvent, cancellationToken);
    }
}