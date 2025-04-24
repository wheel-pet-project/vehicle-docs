using Application.UseCases.Queries.GetOsagoByVehicleDocumentsId;
using Domain.OsagoAggregate;
using Domain.SharedKernel.Errors;
using Domain.VehicleDocumentsAggregate;
using JetBrains.Annotations;
using Npgsql;
using Xunit;

namespace IntegrationTests.Queries;

[TestSubject(typeof(GetOsagoByVehicleDocumentsIdQueryHandler))]
public class GetOsagoByVehicleDocumentsIdQueryHandlerShould : IntegrationTestBase
{
    private readonly string _yandexS3TestHost = "yandex.testhost";

    private readonly VehicleDocuments _vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid(), Guid.NewGuid());

    private readonly string _photoBucketAndKey = "photoBucketAndKey";
    private readonly DateOnly _dateOfIssue = DateOnly.FromDateTime(DateTime.Now);
    private readonly DateOnly _dateOfExpiry = DateOnly.FromDateTime(DateTime.Now.AddYears(1));


    [Fact]
    public async Task ReturnOsagoWithCorrectValues()
    {
        // Arrange
        var vehicleDocumentsId = await AddVehicleDocuments(_vehicleDocuments);

        var expectedOsago = Osago.Create(vehicleDocumentsId, _photoBucketAndKey, _dateOfIssue,
            _dateOfExpiry);
        Context.Attach(expectedOsago.ExpiryStatus);
        await Context.Osagos.AddAsync(expectedOsago, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetOsagoByVehicleDocumentsIdQuery(vehicleDocumentsId);
        var queryHandlerBuilder = new QueryHandlerBuilder();
        var queryHandler = queryHandlerBuilder.Build(DataSource, _yandexS3TestHost);

        // Act
        var actual = await queryHandler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(actual.Value);
        Assert.Contains(expectedOsago.PhotoStorageBucketAndKey, actual.Value.PhotoS3Url);
        Assert.Contains(_yandexS3TestHost, actual.Value.PhotoS3Url);
        Assert.Equal(expectedOsago.ExpiryStatus, actual.Value.ExpiryStatus);
        Assert.Equal(expectedOsago.DateOfIssue, actual.Value.DateOfIssue);
        Assert.Equal(expectedOsago.DateOfExpiry, actual.Value.DateOfExpiry);
    }

    [Fact]
    public async Task ReturnNotFoundErrorIfPtsNotFound()
    {
        // Arrange
        var vehicleDocumentsId = await AddVehicleDocuments(_vehicleDocuments);

        var query = new GetOsagoByVehicleDocumentsIdQuery(vehicleDocumentsId);
        var queryHandlerBuilder = new QueryHandlerBuilder();
        var queryHandler = queryHandlerBuilder.Build(DataSource, _yandexS3TestHost);

        // Act
        var actual = await queryHandler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(actual.IsSuccess);
        Assert.Contains(actual.Errors, x => x is NotFound);
    }

    private async Task<Guid> AddVehicleDocuments(VehicleDocuments vehicleDocuments)
    {
        await Context.VehicleDocuments.AddAsync(vehicleDocuments);
        await Context.SaveChangesAsync();

        return vehicleDocuments.Id;
    }

    private class QueryHandlerBuilder
    {
        public GetOsagoByVehicleDocumentsIdQueryHandler Build(NpgsqlDataSource dataSource, string yandexTestHost)
        {
            return new GetOsagoByVehicleDocumentsIdQueryHandler(dataSource, yandexTestHost);
        }
    }
}