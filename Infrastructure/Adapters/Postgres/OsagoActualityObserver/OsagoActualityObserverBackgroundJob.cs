using System.Linq.Expressions;
using Application.Ports.Postgres;
using Domain.OsagoAggregate;
using Domain.SharedKernel.Errors;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Adapters.Postgres.OsagoActualityObserver;

public class OsagoActualityObserverBackgroundJob(
    IOsagoRepository osagoRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<OsagoActualityObserverBackgroundJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        var expiredOsagos = await osagoRepository.GetAll(IsExpired());

        if (expiredOsagos.Count > 0)
            foreach (var osago in expiredOsagos)
                try
                {
                    osago.Expire(timeProvider);
                    osagoRepository.Update(osago);

                    var commitResult = await unitOfWork.Commit();
                    if (commitResult.IsFailed) throw ((CommitFail)commitResult.Errors[0]).Exception;
                }
                catch (Exception e)
                {
                    logger.LogCritical(
                        "Fail to process update osago expiry status in domain event handler, exception: {e}", e);
                }
    }

    private Expression<Func<Osago, bool>> IsExpired()
    {
        return x => x.DateOfExpiry.ToDateTime(new TimeOnly()) < timeProvider.GetUtcNow().DateTime;
    }
}