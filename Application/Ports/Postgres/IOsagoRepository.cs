using Domain.OsagoAggregate;

namespace Application.Ports.Postgres;

public interface IOsagoRepository
{
    Task Add(Osago osago);

    Task<Osago?> GetById(Guid id);

    void Update(Osago osago);
}