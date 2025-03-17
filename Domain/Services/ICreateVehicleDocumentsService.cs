using Domain.VehicleDocumentsAggregate;

namespace Domain.Services;

public interface ICreateVehicleDocumentsService
{
    VehicleDocuments Create(VehicleDocuments? existingVehicleDocuments, Guid vehicleId);
}