using System.Collections.Concurrent;
using System.Data;
using Dapper;
using Domain.SharedKernel.Exceptions.InternalExceptions.AlreadyHaveThisState;
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
        ContractResolver = new PrivateSetterAndCtorContractResolver()
    };

    public async Task Execute(IJobExecutionContext jobExecutionContext)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var inboxEvents = (await connection.QueryAsync<InboxEvent>(QuerySql, transaction))
            .AsList()
            .AsReadOnly();

        if (inboxEvents.Count > 0)
        {
            var updateQueue = new ConcurrentQueue<EventUpdate>();

            var consumerEvents = inboxEvents
                .Select(ev =>
                    JsonConvert.DeserializeObject<IConvertibleToCommand>(ev.Content,
                        _jsonSerializerSettings))
                .OfType<IConvertibleToCommand>()
                .AsList()
                .AsReadOnly();

            var sendTasks = consumerEvents
                .Select(domainEvent => SendToMediator(domainEvent, updateQueue,
                    jobExecutionContext.CancellationToken))
                .ToList()
                .AsReadOnly();

            await Task.WhenAll(sendTasks);

            var updates = updateQueue.ToList();
            var formattedSql = FormatSql(updates);
            var parameters = GenerateDynamicParameters(updates);

            await connection.ExecuteAsync(formattedSql, parameters, transaction);

            await transaction.CommitAsync();
        }

        return;

        async Task SendToMediator(
            IConvertibleToCommand @event,
            ConcurrentQueue<EventUpdate> updateQueue,
            CancellationToken cancellationToken)
        {
            try
            {
                var processingResult = await mediator.Send(@event.ToCommand(), cancellationToken);
                if (processingResult.IsSuccess)
                    updateQueue.Enqueue(new EventUpdate(@event.EventId, DateTime.UtcNow));
            }
            catch (AlreadyHaveThisStateException)
            {
                updateQueue.Enqueue(new EventUpdate(@event.EventId, DateTime.UtcNow));
            }
            catch (Exception e)
            {
                updateQueue.Enqueue(new EventUpdate(@event.EventId));
                logger.LogError("Fail in processing inbox events, exception: {e}", e);
            }
        }
    }

    private string FormatSql(List<EventUpdate> updates)
    {
        var paramNames = string.Join(",",
            updates.Select((_, i) => $"(@EventId{i}, @ProcessedOnUtc{i})"));
        var formattedSql = string.Format(UpdateSql, paramNames);

        return formattedSql;
    }

    private DynamicParameters GenerateDynamicParameters(List<EventUpdate> updates)
    {
        var parameters = new DynamicParameters();
        for (var i = 0; i < updates.Count; i++)
        {
            parameters.Add($"EventId{i}", updates[i].EventId);
            parameters.Add($"ProcessedOnUtc{i}", (object?)updates[i].ProcessedOnUtc ?? DBNull.Value, DbType.DateTime);
        }

        return parameters;
    }

    private class EventUpdate(Guid eventId, DateTime? processedOnUtc = null)
    {
        public Guid EventId { get; } = eventId;
        public DateTime? ProcessedOnUtc { get; } = processedOnUtc;
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