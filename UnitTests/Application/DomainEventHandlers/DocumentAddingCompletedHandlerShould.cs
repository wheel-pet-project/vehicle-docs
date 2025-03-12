using Application.DomainEventHandlers;
using Application.Ports.Kafka;
using Domain.VehicleDocumentsAggregate.DomainEvents;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.DomainEventHandlers;

[TestSubject(typeof(DocumentAddingCompletedHandler))]
public class DocumentAddingCompletedHandlerShould
{
    private readonly DocumentAddingCompletedDomainEvent _event = new(Guid.NewGuid());

    private readonly Mock<IMessageBus> _messageBusMock = new();

    [Fact]
    public async Task CallMessageBusPublish()
    {
        // Arrange
        var handler = new DocumentAddingCompletedHandler(_messageBusMock.Object);

        // Act
        await handler.Handle(_event, TestContext.Current.CancellationToken);

        // Assert
        _messageBusMock.Verify(
            x => x.Publish(It.IsAny<DocumentAddingCompletedDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}