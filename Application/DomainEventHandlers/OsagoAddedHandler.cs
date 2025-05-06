using Application.Ports.Postgres;
using Domain.OsagoAggregate.DomainEvents;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.Exceptions.InternalExceptions;
using MediatR;

namespace Application.DomainEventHandlers;

public class OsagoAddedHandler(
    IVehicleDocumentsRepository vehicleDocumentsRepository,
    IUnitOfWork unitOfWork)
    : INotificationHandler<OsagoAddedDomainEvent>
{
    public async Task Handle(OsagoAddedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var vehicleDocuments = await vehicleDocumentsRepository.GetById(domainEvent.VehicleDocumentsId);
        if (vehicleDocuments == null)
            throw new DataConsistencyViolationException("Osago added for not existing vehicle documents");

        vehicleDocuments.MarkAsOsagoAdded();

        vehicleDocumentsRepository.Update(vehicleDocuments);

        var commitResult = await unitOfWork.Commit();
        if (commitResult.IsFailed) throw ((CommitFail)commitResult.Errors[0]).Exception;
    }
}