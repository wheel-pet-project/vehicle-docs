using Application.Ports.Postgres;
using Domain.Services;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddVehicleDocuments;

public class AddVehicleDocumentsHandler(
    ICreateVehicleDocumentsService createVehicleDocumentsService,
    IVehicleDocumentsRepository vehicleDocumentsRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddVehicleDocumentsCommand, Result>
{
    public async Task<Result> Handle(AddVehicleDocumentsCommand command, CancellationToken _)
    {
        var existingVehicleDocuments = await vehicleDocumentsRepository.GetByVehicleId(command.VehicleId);
        
        var vehicleDocuments = createVehicleDocumentsService.Create(
            existingVehicleDocuments, 
            command.SagaId, 
            command.VehicleId);

        await vehicleDocumentsRepository.Add(vehicleDocuments);

        return await unitOfWork.Commit();
    }
}