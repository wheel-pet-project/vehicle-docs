using Domain.OsagoAggregate.DomainEvents;
using MediatR;

namespace Application.DomainEventHandlers;

public class OsagoExpiredHandler : INotificationHandler<OsagoExpiredDomainEvent>
{
    public Task Handle(OsagoExpiredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}