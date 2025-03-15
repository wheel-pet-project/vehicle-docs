using Infrastructure.Adapters.Postgres.Inbox.InputConsumerEvents;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;

namespace Infrastructure.Adapters.Postgres.Inbox;

public class Inbox(DataContext context) : IInbox
{
    private const int DuplicateKeyCode = 23505; // duplicate key value violates unique constraint code

    private readonly JsonSerializerSettings _jsonSettings = new() { TypeNameHandling = TypeNameHandling.All };

    public async Task<bool> Save(IInputConsumerEvent inputConsumerEvent)
    {
        var inboxEvent = new InboxEvent
        {
            EventId = inputConsumerEvent.EventId,
            Type = inputConsumerEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(inputConsumerEvent, _jsonSettings),
            OccurredOnUtc = DateTime.UtcNow
        };

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await context.Inbox.AddAsync(inboxEvent);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { ErrorCode: DuplicateKeyCode })
        {
            return true;
        }
        catch
        {
            return false;
        }
    }
}