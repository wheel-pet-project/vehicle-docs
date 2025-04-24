using Domain.SharedKernel.Exceptions.AlreadyHaveThisState;
using Domain.VehicleDocumentsAggregate;

namespace Domain.Services;

public class CreateVehicleDocumentsService : ICreateVehicleDocumentsService
{
    public VehicleDocuments Create(VehicleDocuments? existingVehicleDocuments, Guid sagaId, Guid vehicleId)
    {
        if (existingVehicleDocuments != null) throw new AlreadyHaveThisStateException(
                $"Vehicle documents already exists for vehicle (vehicle id: {vehicleId})");
        
        return VehicleDocuments.Create(sagaId, vehicleId);
    }
}