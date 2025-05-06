using System.Linq.Expressions;
using Domain.OsagoAggregate;

namespace Application.Ports.Postgres;

public interface IOsagoRepository
{
    Task Add(Osago osago);

    Task<List<Osago>> GetAll(Expression<Func<Osago, bool>> predicate);

    Task<Osago?> GetByVehicleDocumentsId(Guid vehicleDocumentsId);

    void Update(Osago osago);
}