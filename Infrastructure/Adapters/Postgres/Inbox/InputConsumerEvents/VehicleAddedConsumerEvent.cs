using Application.UseCases.Commands.AddVehicleDocuments;
using FluentResults;
using MediatR;

namespace Infrastructure.Adapters.Postgres.Inbox.InputConsumerEvents;

public class VehicleAddedConsumerEvent(Guid eventId, Guid vehicleId) : IInputConsumerEvent
{
    public Guid EventId { get; } = eventId;
    public Guid VehicleId { get; } = vehicleId;

    public IRequest<Result> ToCommand()
    {
        return new AddVehicleDocumentsCommand(VehicleId);
    }
}