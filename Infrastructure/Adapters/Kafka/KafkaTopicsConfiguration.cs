namespace Infrastructure.Adapters.Kafka;

public class KafkaTopicsConfiguration
{
    public required string OsagoExpiredTopic { get; set; }
    public required string DocumentAddingCompletedTopic { get; set; }
}