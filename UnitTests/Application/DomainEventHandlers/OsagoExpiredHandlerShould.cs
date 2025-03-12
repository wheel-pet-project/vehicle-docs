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
    private readonly OsagoExpiredDomainEvent _event = new(Guid.NewGuid());

    private readonly Mock<IMessageBus> _messageBusMock = new();

    [Fact]
    public async Task CallMessageBusPublish()
    {
        // Arrange
        var handler = new OsagoExpiredHandler(_messageBusMock.Object);

        // Act
        await handler.Handle(_event, TestContext.Current.CancellationToken);

        // Assert
        _messageBusMock.Verify(x => x.Publish(It.IsAny<OsagoExpiredDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}