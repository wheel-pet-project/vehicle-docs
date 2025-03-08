using Domain.OsagoAggregate.DomainEvents;
using MediatR;

namespace Application.DomainEventHandlers;

public class OsagoAddedHandler : INotificationHandler<OsagoAddedDomainEvent>
{
    public Task Handle(OsagoAddedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}