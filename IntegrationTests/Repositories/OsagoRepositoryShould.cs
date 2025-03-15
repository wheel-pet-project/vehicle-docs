using Domain.OsagoAggregate;
using Domain.VehicleDocumentsAggregate;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Repositories;
using JetBrains.Annotations;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace IntegrationTests.Repositories;

[TestSubject(typeof(OsagoRepository))]
public class OsagoRepositoryShould : IntegrationTestBase
{
    private readonly VehicleDocuments _vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

    [Fact]
    public async Task Add()
    {
        // Arrange
        await AddVehicleDocuments(_vehicleDocuments, Context);

        var osago = Osago.Create(_vehicleDocuments.Id, "photoStorageBucketAndKey",
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1), DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1));
        var (repository, uow) = RepositoryAndUnitOfWorkBuilder.Build(Context);

        // Act
        await repository.Add(osago);
        await uow.Commit();

        // Assert
        osago.ClearDomainEvents();
        var osagoFromDb = await repository.GetByVehicleDocumentsId(_vehicleDocuments.Id);
        Assert.NotNull(osagoFromDb);
        Assert.Equal(osago, osagoFromDb);
    }

    [Fact]
    public async Task Update()
    {
        // Arrange
        await AddVehicleDocuments(_vehicleDocuments, Context);

        var osago = Osago.Create(_vehicleDocuments.Id, "photoStorageBucketAndKey",
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1), DateOnly.FromDateTime(DateTime.UtcNow));

        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetUtcNow(DateTime.UtcNow.AddDays(1));

        var (repositoryForArrange, uowForArrange) = RepositoryAndUnitOfWorkBuilder.Build(Context);
        await repositoryForArrange.Add(osago);
        await uowForArrange.Commit();
        var (repository, uow) = RepositoryAndUnitOfWorkBuilder.Build(Context);
        var osagoFromDb = await repository.GetByVehicleDocumentsId(_vehicleDocuments.Id);
        osagoFromDb.Expire(fakeTimeProvider);

        // Act
        repository.Update(osagoFromDb!);
        await uow.Commit();

        // Assert
        osago.ClearDomainEvents();
        var osagoFromDbAfterUpdate = await repositoryForArrange.GetByVehicleDocumentsId(_vehicleDocuments.Id);
        Assert.NotNull(osagoFromDbAfterUpdate);
        Assert.Equal(osago, osagoFromDbAfterUpdate);
    }

    [Fact]
    public async Task GetByVehicleDocumentsId()
    {
        // Arrange
        await AddVehicleDocuments(_vehicleDocuments, Context);

        var osago = Osago.Create(_vehicleDocuments.Id, "photoStorageBucketAndKey",
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1), DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1));

        var (repositoryForArrange, uowForArrange) = RepositoryAndUnitOfWorkBuilder.Build(Context);
        await repositoryForArrange.Add(osago);
        await uowForArrange.Commit();
        var (repository, _) = RepositoryAndUnitOfWorkBuilder.Build(Context);

        // Act
        var actual = await repository.GetByVehicleDocumentsId(_vehicleDocuments.Id);

        // Assert
        osago.ClearDomainEvents();
        Assert.NotNull(actual);
        Assert.Equal(osago, actual);
    }

    private async Task AddVehicleDocuments(VehicleDocuments vehicleDocuments, DataContext context)
    {
        await context.VehicleDocuments.AddAsync(vehicleDocuments);
        await context.SaveChangesAsync();
    }

    private static class RepositoryAndUnitOfWorkBuilder
    {
        public static (OsagoRepository, UnitOfWork) Build(DataContext context)
        {
            return (new OsagoRepository(context), new UnitOfWork(context));
        }
    }
}