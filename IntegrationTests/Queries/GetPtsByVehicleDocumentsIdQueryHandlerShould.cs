using Application.UseCases.Queries.DapperMappingExtensions;
using Application.UseCases.Queries.GetPtsByVehicleDocumentsId;
using Dapper;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.ValueObjects;
using Domain.VehicleDocumentsAggregate;
using JetBrains.Annotations;
using Npgsql;
using Xunit;

namespace IntegrationTests.Queries;

[TestSubject(typeof(GetPtsByVehicleDocumentsIdQueryHandler))]
public class GetPtsByVehicleDocumentsIdQueryHandlerShould : IntegrationTestBase
{
    private readonly string _yandexS3TestHost = "yandex.testhost";

    private readonly VehicleDocuments _vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

    private readonly Pts _pts = Pts.Create(new string('*', 10), new string('*', 10),
        DateOnly.FromDateTime(DateTime.UtcNow),
        Color.Beige, Vin.Create("SALYA2BN2KA791786"));

    [Fact]
    public async Task ReturnPtsWithCorrectValues()
    {
        SqlMapper.AddTypeHandler(new DateOnlyMapper());
        var query = new GetPtsByVehicleDocumentsIdQuery(_vehicleDocuments.Id);
        _vehicleDocuments.AddPts(_pts);
        await Context.VehicleDocuments.AddAsync(_vehicleDocuments, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queryBuilder = new QueryHandlerBuilder();
        var queryHandler = queryBuilder.Build(DataSource, _yandexS3TestHost);

        // Act
        var actual = await queryHandler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(actual.Value);
        Assert.Contains(_vehicleDocuments.Pts!.FrontPhotoStorageBucketAndKey, actual.Value.FrontPhotoS3Url);
        Assert.Contains(_yandexS3TestHost, actual.Value.FrontPhotoS3Url);
        Assert.Contains(_vehicleDocuments.Pts!.BackPhotoStorageBucketAndKey, actual.Value.BackPhotoS3Url);
        Assert.Contains(_yandexS3TestHost, actual.Value.BackPhotoS3Url);
        Assert.Equal(_vehicleDocuments.Pts!.YearOfManufacture, actual.Value.YearOfManufacture);
        Assert.Equal(_vehicleDocuments.Pts.Color, actual.Value.Color);
        Assert.Equal(_vehicleDocuments.Pts!.Vin.Number, actual.Value.Vin);
    }

    [Fact]
    public async Task ReturnNotFoundErrorIfPtsNotFound()
    {
        SqlMapper.AddTypeHandler(new DateOnlyMapper());
        var query = new GetPtsByVehicleDocumentsIdQuery(_vehicleDocuments.Id);

        await Context.VehicleDocuments.AddAsync(_vehicleDocuments, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queryBuilder = new QueryHandlerBuilder();
        var queryHandler = queryBuilder.Build(DataSource, _yandexS3TestHost);

        // Act
        var actual = await queryHandler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(actual.IsSuccess);
        Assert.Contains(actual.Errors, x => x is NotFound);
    }

    private class QueryHandlerBuilder
    {
        public GetPtsByVehicleDocumentsIdQueryHandler Build(NpgsqlDataSource dataSource, string yandexTestHost)
        {
            return new GetPtsByVehicleDocumentsIdQueryHandler(dataSource, yandexTestHost);
        }
    }
}