using Application.UseCases.Queries.GetVehicleDocumentsByVehicleId;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.ValueObjects;
using Domain.VehicleDocumentsAggregate;
using JetBrains.Annotations;
using Npgsql;
using Xunit;

namespace IntegrationTests.Queries;

[TestSubject(typeof(GetVehicleDocumentsByVehicleIdQueryHandler))]
public class GetVehicleDocumentsByVehicleIdQueryHandlerShould : IntegrationTestBase
{
    private readonly GetVehicleDocumentsByVehicleIdQuery _query = new(Guid.NewGuid());
    private readonly Sts _sts = Sts.Create(new string('*', 10), new string('*', 10));

    private readonly Pts _pts = Pts.Create(new string('*', 10), new string('*', 10),
        DateOnly.FromDateTime(DateTime.UtcNow),
        Color.Beige, Vin.Create("SALYA2BN2KA791786"));

    [Fact]
    public async Task ReturnVehicleDocumentWithCorrectValues()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_query.VehicleId);
        vehicleDocuments.MarkAsOsagoAdded();
        vehicleDocuments.AddPts(_pts);
        vehicleDocuments.AddSts(_sts);

        await Context.VehicleDocuments.AddAsync(vehicleDocuments, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var queryBuilder = new QueryHandlerBuilder();
        var queryHandler = queryBuilder.Build(DataSource);

        // Act
        var actual = await queryHandler.Handle(_query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(vehicleDocuments.Id, actual.Value.VehicleDocumentsId);
        Assert.Equal(vehicleDocuments.VehicleId, actual.Value.VehicleId);
        Assert.Equivalent(vehicleDocuments.Status, actual.Value.Status);
    }

    [Fact]
    public async Task ReturnNotFoundErrorIfVehicleDocumentsNotFound()
    {
        // Arrange
        var queryBuilder = new QueryHandlerBuilder();
        var queryHandler = queryBuilder.Build(DataSource);

        // Act
        var actual = await queryHandler.Handle(_query, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(actual.IsSuccess);
        Assert.Contains(actual.Errors, x => x is NotFound);
    }

    private class QueryHandlerBuilder
    {
        public GetVehicleDocumentsByVehicleIdQueryHandler Build(NpgsqlDataSource dataSource)
        {
            return new GetVehicleDocumentsByVehicleIdQueryHandler(dataSource);
        }
    }
}