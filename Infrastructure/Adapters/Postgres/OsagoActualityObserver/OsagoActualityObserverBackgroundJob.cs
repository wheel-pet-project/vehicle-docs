using Dapper;
using Domain.OsagoAggregate;
using Domain.OsagoAggregate.DomainEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;

namespace Infrastructure.Adapters.Postgres.OsagoActualityObserver;

public class OsagoActualityObserverBackgroundJob(
    NpgsqlDataSource dataSource,
    IMediator mediator,
    TimeProvider timeProvider,
    ILogger<OsagoActualityObserverBackgroundJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        List<ExpiredOsagoDapperModel> expiredOsagos;
        
        await using (var connection = await dataSource.OpenConnectionAsync())
        {
            expiredOsagos = (await connection.QueryAsync<ExpiredOsagoDapperModel>(Sql,
                    new { Today = timeProvider.GetUtcNow().DateTime, NotExpiredStatusId = ExpiryStatus.NotExpired.Id }))
                .AsList();
        }
        
        if (expiredOsagos.Count > 0)
            foreach (var osago in expiredOsagos)
                try
                {
                    await mediator.Publish(new OsagoExpiredDomainEvent(osago.VehicleDocumentsId));
                }
                catch (Exception e)
                {
                    logger.LogCritical(
                        "Fail to process update osago expiry status in domain event handler, exception: {e}", e);
                }
    }
    
    private record ExpiredOsagoDapperModel(Guid VehicleDocumentsId, DateOnly DateOfExpiry);

    private const string Sql =
        """
        SELECT vehicle_documents_id AS VehicleDocumentsId, 
               date_of_expiry AS DateOfExpiry
        FROM osago
        WHERE date_of_expiry < @Today AND 
              expiry_status_id = @NotExpiredStatusId
        ORDER BY date_of_expiry
        LIMIT 100
        """;
}