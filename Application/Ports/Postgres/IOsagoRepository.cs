using Domain.OsagoAggregate;

namespace Application.Ports.Postgres;

public interface IOsagoRepository
{
    Task Add(Osago osago);

    Task<Osago?> GetByVehicleDocumentsId(Guid vehicleDocumentsId);

    void Update(Osago osago);
}