using Application.UseCases.Commands.AddVehicleDocuments;
using FluentResults;
using MediatR;

namespace Infrastructure.Adapters.Postgres.Inbox.InputConsumerEvents;

public class VehicleAddedConsumerEvent(Guid eventId, Guid sagaId, Guid vehicleId) : IConvertibleToCommand
{
    public Guid EventId { get; } = eventId;
    public Guid SagaId { get; } = sagaId;
    public Guid VehicleId { get; } = vehicleId;

    public IRequest<Result> ToCommand()
    {
        return new AddVehicleDocumentsCommand(SagaId, VehicleId);
    }
}