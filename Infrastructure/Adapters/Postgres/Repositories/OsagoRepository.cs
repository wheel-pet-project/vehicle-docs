using Application.Ports.Postgres;
using Domain.OsagoAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class OsagoRepository(DataContext context) : IOsagoRepository
{
    public async Task Add(Osago osago)
    {
        context.Attach(osago.ExpiryStatus);

        await context.Osagos.AddAsync(osago);
    }

    public async Task<Osago?> GetById(Guid id)
    {
        return await context.Osagos
            .Include(x => x.ExpiryStatus)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Update(Osago osago)
    {
        context.Attach(osago.ExpiryStatus);

        context.Osagos.Update(osago);
    }
}