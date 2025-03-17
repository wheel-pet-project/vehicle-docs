using Domain.VehicleDocumentsAggregate;

namespace Application.Ports.Postgres;

public interface IVehicleDocumentsRepository
{
    Task Add(VehicleDocuments vehicleDocuments);

    Task<VehicleDocuments?> GetById(Guid id);

    Task<VehicleDocuments?> GetByVehicleId(Guid vehicleId);

    void Update(VehicleDocuments vehicleDocuments);
}