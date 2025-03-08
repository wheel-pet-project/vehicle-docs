using Application.Ports.Postgres;
using Domain.VehicleDocumentsAggregate;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class VehicleDocumentsRepository(DataContext context) : IVehicleDocumentsRepository
{
    public async Task Add(VehicleDocuments vehicleDocuments)
    {
        await context.VehicleDocuments.AddAsync(vehicleDocuments);
    }

    public async Task<VehicleDocuments?> GetById(Guid id)
    {
        return await context.VehicleDocuments.FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Update(VehicleDocuments vehicleDocuments)
    {
        context.VehicleDocuments.Update(vehicleDocuments);
    }
}