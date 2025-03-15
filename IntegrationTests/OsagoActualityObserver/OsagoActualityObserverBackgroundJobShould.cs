using Domain.OsagoAggregate;
using Domain.VehicleDocumentsAggregate;
using Infrastructure.Adapters.Postgres.OsagoActualityObserver;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Npgsql;
using Quartz;
using Xunit;

namespace IntegrationTests.OsagoActualityObserver;

[TestSubject(typeof(OsagoActualityObserverBackgroundJob))]
public class OsagoActualityObserverBackgroundJobShould : IntegrationTestBase
{
    [Fact]
    public async Task CallMediatorIfFoundExpiredOsago()
    {
        // Arrange
        await AddExpiredOsago();

        var timeProvider = TimeProvider.System;
        var jobBuilder = new JobBuilder();
        var job = jobBuilder.Build(DataSource, timeProvider);
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        jobBuilder.VerifyMediatorCalls(1);
    }

    [Fact]
    public async Task NotCallMediatorIfNotFoundExpiredOsago()
    {
        var timeProvider = TimeProvider.System;
        var jobBuilder = new JobBuilder();
        var job = jobBuilder.Build(DataSource, timeProvider);
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        jobBuilder.VerifyMediatorCalls(0);
    }
    
    async Task AddExpiredOsago()
    {
        var vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

        await Context.VehicleDocuments.AddAsync(vehicleDocuments);
        await Context.SaveChangesAsync();
        
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(-1));

        var osago = Osago.Create(vehicleDocuments.Id, "photoStorageBucketAndKey",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow));

        Context.Attach(osago.ExpiryStatus);
        Context.Osagos.Add(osago);
        await Context.SaveChangesAsync();
    }
    
    private class JobBuilder
    {
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ILogger<OsagoActualityObserverBackgroundJob>> _loggerMock = new();

        public OsagoActualityObserverBackgroundJob Build(NpgsqlDataSource dataSource, TimeProvider timeProvider)
        {
            return new OsagoActualityObserverBackgroundJob(dataSource, _mediatorMock.Object, timeProvider,
                _loggerMock.Object);
        }

        public void VerifyMediatorCalls(int times)
        {
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Exactly(times));
        }
    }
}