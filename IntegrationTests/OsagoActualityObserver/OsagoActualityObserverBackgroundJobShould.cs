using Domain.OsagoAggregate;
using Domain.VehicleDocumentsAggregate;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.OsagoActualityObserver;
using Infrastructure.Adapters.Postgres.Repositories;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Xunit;

namespace IntegrationTests.OsagoActualityObserver;

[TestSubject(typeof(OsagoActualityObserverBackgroundJob))]
public class OsagoActualityObserverBackgroundJobShould : IntegrationTestBase
{
    private readonly Mock<ILogger<OsagoActualityObserverBackgroundJob>> _loggerMock = new();
    private readonly Mock<IJobExecutionContext> _jobExecutionContextMock = new();

    [Fact]
    public async Task ChangeStatusOfOsagoInDbIfExistExpired()
    {
        // Arrange
        await AddExpiredOsago();
        var job = new OsagoActualityObserverBackgroundJob(new OsagoRepository(Context), new UnitOfWork(Context),
            TimeProvider.System, _loggerMock.Object);

        // Act
        await job.Execute(_jobExecutionContextMock.Object);

        // Assert
        var osagoFromDb = await Context.Osagos.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ExpiryStatus.Expired, osagoFromDb?.ExpiryStatus);
    }

    [Fact]
    public async Task NotChangingIfNotExistExpired()
    {
        // Arrange
        await AddNotExpiredOsago();
        var job = new OsagoActualityObserverBackgroundJob(new OsagoRepository(Context), new UnitOfWork(Context),
            TimeProvider.System, _loggerMock.Object);

        // Act
        await job.Execute(_jobExecutionContextMock.Object);

        // Assert
        var osagoFromDb = await Context.Osagos.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.Equal(ExpiryStatus.NotExpired, osagoFromDb?.ExpiryStatus);
    }

    private async Task AddExpiredOsago()
    {
        var vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid(), Guid.NewGuid());
        var osago = Osago.Create(vehicleDocuments.Id, "photoStorageBucketAndKey",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        await AddVehicleDocumentsAndOsago(vehicleDocuments, osago);
    }

    private async Task AddNotExpiredOsago()
    {
        var vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid(), Guid.NewGuid());
        var osago = Osago.Create(vehicleDocuments.Id, "photoStorageBucketAndKey",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)));

        await AddVehicleDocumentsAndOsago(vehicleDocuments, osago);
    }

    private async Task AddVehicleDocumentsAndOsago(VehicleDocuments vehicleDocuments, Osago osago)
    {
        await Context.VehicleDocuments.AddAsync(vehicleDocuments);
        await Context.SaveChangesAsync();

        Context.Attach(osago.ExpiryStatus);
        Context.Osagos.Add(osago);
        await Context.SaveChangesAsync();
    }
}