using Domain.VehicleDocumentsAggregate;

namespace Application.Ports.Postgres;

public interface IVehicleDocumentsRepository
{
    Task Add(VehicleDocuments vehicleDocuments);

    Task<VehicleDocuments?> GetById(Guid id);

    void Update(VehicleDocuments vehicleDocuments);
}