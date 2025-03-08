using Domain.VehicleDocumentsAggregate.DomainEvents;
using MediatR;

namespace Application.DomainEventHandlers;

public class DocumentAddingCompletedHandler : INotificationHandler<DocumentAddingCompletedDomainEvent>
{
    public Task Handle(DocumentAddingCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}