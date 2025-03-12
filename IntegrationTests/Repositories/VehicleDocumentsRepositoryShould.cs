using Domain.VehicleDocumentsAggregate;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Repositories;
using JetBrains.Annotations;
using Xunit;

namespace IntegrationTests.Repositories;

[TestSubject(typeof(VehicleDocumentsRepository))]
public class VehicleDocumentsRepositoryShould : IntegrationTestBase
{
    private readonly VehicleDocuments _vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

    [Fact]
    public async Task Add()
    {
        // Arrange
        var (repository, uow) = RepositoryAndUnitOfWorkBuilder.Build(Context);

        // Act
        await repository.Add(_vehicleDocuments);
        await uow.Commit();

        // Assert
        _vehicleDocuments.ClearDomainEvents();
        var vehicleDocumentsFromDb = await repository.GetById(_vehicleDocuments.Id);
        Assert.NotNull(vehicleDocumentsFromDb);
        Assert.Equal(_vehicleDocuments, vehicleDocumentsFromDb);
    }

    [Fact]
    public async Task Update()
    {
        // Arrange
        var (repositoryForArrange, uowForArrange) = RepositoryAndUnitOfWorkBuilder.Build(Context);
        await repositoryForArrange.Add(_vehicleDocuments);
        await uowForArrange.Commit();
        var vehicleDocumentsFromDb = await repositoryForArrange.GetById(_vehicleDocuments.Id);
        vehicleDocumentsFromDb!.MarkAsOsagoAdded();
        var (repository, uow) = RepositoryAndUnitOfWorkBuilder.Build(Context);

        // Act
        repository.Update(vehicleDocumentsFromDb);
        await uow.Commit();

        // Assert
        _vehicleDocuments.ClearDomainEvents();
        var vehicleDocumentsFromDbAfterUpdate = await repository.GetById(_vehicleDocuments.Id);
        Assert.NotNull(vehicleDocumentsFromDbAfterUpdate);
        Assert.Equal(_vehicleDocuments, vehicleDocumentsFromDbAfterUpdate);
    }

    [Fact]
    public async Task GetById()
    {
        // Arrange
        var (repository, uow) = RepositoryAndUnitOfWorkBuilder.Build(Context);
        await repository.Add(_vehicleDocuments);
        await uow.Commit();

        // Act
        var actual = await repository.GetById(_vehicleDocuments.Id);

        // Assert
        _vehicleDocuments.ClearDomainEvents();
        Assert.NotNull(actual);
        Assert.Equal(_vehicleDocuments, actual);
    }

    private static class RepositoryAndUnitOfWorkBuilder
    {
        public static (VehicleDocumentsRepository, UnitOfWork) Build(DataContext context)
        {
            return (new VehicleDocumentsRepository(context), new UnitOfWork(context));
        }
    }
}