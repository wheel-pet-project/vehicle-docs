using System.Collections.Concurrent;
using Dapper;
using Domain.SharedKernel.Exceptions.AlreadyHaveThisState;
using Infrastructure.Adapters.Postgres.Inbox.InputConsumerEvents;
using JsonNet.ContractResolvers;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Quartz;

namespace Infrastructure.Adapters.Postgres.Inbox;

[DisallowConcurrentExecution]
public class InboxBackgroundJob(
    NpgsqlDataSource dataSource,
    IMediator mediator,
    ILogger<InboxBackgroundJob> logger)
    : IJob
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new PrivateSetterContractResolver()
    };

    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        var inboxEvents = (await connection.QueryAsync<InboxEvent>(QuerySql)).AsList().AsReadOnly();

        if (inboxEvents.Count > 0)
        {
            var updateQueue = new ConcurrentQueue<Guid>();

            var consumerEvents = inboxEvents
                .Select(ev => JsonConvert.DeserializeObject<IInputConsumerEvent>(ev.Content, _jsonSerializerSettings))
                .OfType<IInputConsumerEvent>()
                .AsList()
                .AsReadOnly();

            var sendTasks = consumerEvents
                .Select(domainEvent => SendToMediator(domainEvent, updateQueue,
                    jobExecutionContext.CancellationToken))
                .ToList()
                .AsReadOnly();

            await Task.WhenAll(sendTasks);

            var updateList = updateQueue.ToList();
            var paramNames = string.Join(",", updateList.Select((_, i) => $"(@EventId{i}, @ProcessedOnUtc{i})"));
            var formattedSql = string.Format(UpdateSql, paramNames);

            var processedTime = DateTime.UtcNow;
            var parameters = new DynamicParameters();
            for (var i = 0; i < updateList.Count; i++)
            {
                parameters.Add($"EventId{i}", updateList[i]);
                parameters.Add($"ProcessedOnUtc{i}", processedTime);
            }

            await using var transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(formattedSql, parameters, transaction);

            await transaction.CommitAsync();
        }

        return;

        async Task SendToMediator(
            IInputConsumerEvent @event,
            ConcurrentQueue<Guid> updateQueue,
            CancellationToken cancellationToken)
        {
            try
            {
                var processingResult = await mediator.Send(@event.ToCommand(), cancellationToken);
                if (processingResult.IsSuccess) updateQueue.Enqueue(@event.EventId);
            }
            catch (AlreadyHaveThisStateException)
            {
                updateQueue.Enqueue(@event.EventId);
            }
            catch (Exception e)
            {
                logger.LogError("Fail in processing inbox events, exception: {e}", e);
            }
        }
    }

    private const string QuerySql =
        """
        SELECT event_id AS EventId, 
               content AS Content
        FROM inbox
        WHERE processed_on_utc IS NULL
        ORDER BY occurred_on_utc
        LIMIT 50
        FOR UPDATE SKIP LOCKED 
        """;

    private const string UpdateSql =
        """
        UPDATE inbox
        SET processed_on_utc = new.processed_on_utc
        FROM (VALUES 
            {0}) AS new(event_id, processed_on_utc)
        WHERE inbox.event_id = new.event_id::uuid
        """;
}