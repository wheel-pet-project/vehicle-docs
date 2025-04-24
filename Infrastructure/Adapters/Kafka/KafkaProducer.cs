using Application.Ports.Kafka;
using Domain.OsagoAggregate.DomainEvents;
using Domain.SharedKernel;
using Domain.VehicleDocumentsAggregate.DomainEvents;
using From.VehicleDocumentsKafkaEvents;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Infrastructure.Adapters.Kafka;

public class KafkaProducer(
    ITopicProducerProvider topicProducerProvider,
    IOptions<KafkaTopicsConfiguration> topicsConfiguration) : IMessageBus
{
    private readonly KafkaTopicsConfiguration _configuration = topicsConfiguration.Value;

    public async Task Publish(OsagoExpiredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var producer = topicProducerProvider.GetProducer<string, OsagoExpired>(
            new Uri($"topic:{_configuration.OsagoExpiredTopic}"));

        await producer.Produce(domainEvent.EventId.ToString(),
            new OsagoExpired(domainEvent.EventId, domainEvent.VehicleDocumentsId),
            SetMessageId<OsagoExpired, OsagoExpiredDomainEvent>(domainEvent), cancellationToken);
    }

    public async Task Publish(DocumentAddingCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var producer = topicProducerProvider.GetProducer<string, DocumentAddingCompleted>(
            new Uri($"topic:{_configuration.DocumentAddingCompletedTopic}"));

        await producer.Produce(domainEvent.EventId.ToString(),
            new DocumentAddingCompleted(domainEvent.EventId, domainEvent.SagaId, domainEvent.VehicleId),
            SetMessageId<DocumentAddingCompleted, DocumentAddingCompletedDomainEvent>(domainEvent), cancellationToken);
    }

    private IPipe<KafkaSendContext<string, TContractEvent>> SetMessageId<TContractEvent, TDomainEvent>(
        TDomainEvent domainEvent)
        where TDomainEvent : DomainEvent
        where TContractEvent : class
    {
        return Pipe.Execute<KafkaSendContext<string, TContractEvent>>(ctx => ctx.MessageId = domainEvent.EventId);
    }
}