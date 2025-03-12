using Application.Ports.Postgres;
using Domain.VehicleDocumentsAggregate;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddVehicleDocuments;

public class AddVehicleDocumentsHandler(
    IVehicleDocumentsRepository vehicleDocumentsRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddVehicleDocumentsCommand, Result>
{
    public async Task<Result> Handle(AddVehicleDocumentsCommand command, CancellationToken _)
    {
        var vehicleDocuments = VehicleDocuments.Create(command.VehicleId);

        await vehicleDocumentsRepository.Add(vehicleDocuments);

        return await unitOfWork.Commit();
    }
}