using Infrastructure.Adapters.Postgres.Inbox.InputConsumerEvents;
using Newtonsoft.Json;

namespace Infrastructure.Adapters.Postgres.Inbox;

public class Inbox(DataContext context) : IInbox
{
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
        catch
        {
            return false;
        }
    }
}