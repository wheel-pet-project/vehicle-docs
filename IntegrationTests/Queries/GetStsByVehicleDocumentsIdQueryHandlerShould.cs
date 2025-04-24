using Application.UseCases.Queries.GetStsByVehicleDocumentsId;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.ValueObjects;
using Domain.VehicleDocumentsAggregate;
using JetBrains.Annotations;
using Npgsql;
using Xunit;

namespace IntegrationTests.Queries;

[TestSubject(typeof(GetStsByVehicleDocumentsIdQueryHandler))]
public class GetStsByVehicleDocumentsIdQueryHandlerShould : IntegrationTestBase
{
    private readonly string _yandexS3TestHost = "yandex.testhost";

    private readonly VehicleDocuments _vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid(), Guid.NewGuid());

    private readonly Sts _sts = Sts.Create(new string('*', 10), new string('*', 10));

    [Fact]
    public async Task ReturnStsWithCorrectValues()
    {
        // Arrange
        var query = new GetStsByVehicleDocumentsIdQuery(_vehicleDocuments.Id);
        _vehicleDocuments.AddSts(_sts);
        await Context.VehicleDocuments.AddAsync(_vehicleDocuments, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var queryBuilder = new QueryHandlerBuilder();
        var queryHandler = queryBuilder.Build(DataSource, _yandexS3TestHost);

        // Act
        var actual = await queryHandler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(actual.Value);
        Assert.Contains(_vehicleDocuments.Sts!.FrontPhotoStorageBucketAndKey, actual.Value.FrontPhotoS3Url);
        Assert.Contains(_yandexS3TestHost, actual.Value.FrontPhotoS3Url);
        Assert.Contains(_vehicleDocuments.Sts!.BackPhotoStorageBucketAndKey, actual.Value.BackPhotoS3Url);
        Assert.Contains(_yandexS3TestHost, actual.Value.BackPhotoS3Url);
    }

    [Fact]
    public async Task ReturnNotFoundErrorIfVehicleDocumentsNotFound()
    {
        // Arrange
        await Context.VehicleDocuments.AddAsync(_vehicleDocuments, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetStsByVehicleDocumentsIdQuery(_vehicleDocuments.Id);
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
        public GetStsByVehicleDocumentsIdQueryHandler Build(NpgsqlDataSource dataSource, string yandexTestHost)
        {
            return new GetStsByVehicleDocumentsIdQueryHandler(dataSource, yandexTestHost);
        }
    }
}