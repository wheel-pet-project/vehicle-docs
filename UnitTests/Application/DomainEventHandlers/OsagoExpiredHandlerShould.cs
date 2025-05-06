using Application.DomainEventHandlers;
using Application.Ports.Kafka;
using Domain.OsagoAggregate.DomainEvents;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.DomainEventHandlers;

[TestSubject(typeof(OsagoExpiredHandler))]
public class OsagoExpiredHandlerShould
{
    private readonly OsagoExpiredDomainEvent _domainEvent = new(Guid.NewGuid());

    private readonly Mock<IMessageBus> _messageBusMock = new();

    private readonly OsagoExpiredHandler _handler;

    public OsagoExpiredHandlerShould()
    {
        _handler = new OsagoExpiredHandler(_messageBusMock.Object);
    }

    [Fact]
    public async Task CallPublishInMessageBus()
    {
        // Arrange

        // Act
        await _handler.Handle(_domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _messageBusMock.Verify(x => x.Publish(It.IsAny<OsagoExpiredDomainEvent>(), It.IsAny<CancellationToken>()));
    }
}