using Application.Ports.Kafka;
using Application.Ports.Postgres;
using Domain.OsagoAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using MediatR;

namespace Application.DomainEventHandlers;

public class OsagoExpiredHandler(
    IOsagoRepository osagoRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus,
    TimeProvider timeProvider) : INotificationHandler<OsagoExpiredDomainEvent>
{
    public async Task Handle(OsagoExpiredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var osago = await osagoRepository.GetByVehicleDocumentsId(domainEvent.VehicleDocumentsId);
        if (osago == null)
            throw new DataConsistencyViolationException(
                $"{nameof(OsagoExpiredDomainEvent)} created for not exist osago");
        
        osago.Expire(timeProvider);
        
        osagoRepository.Update(osago);
        
        var commitResult = await unitOfWork.Commit();
        if (commitResult.IsFailed) throw new TaskCanceledException("Could not commit updates");
        
        await messageBus.Publish(domainEvent, cancellationToken);
    }
}