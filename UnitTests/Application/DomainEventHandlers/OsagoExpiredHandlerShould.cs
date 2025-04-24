using Application.DomainEventHandlers;
using Application.Ports.Kafka;
using Application.Ports.Postgres;
using Domain.OsagoAggregate;
using Domain.OsagoAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace UnitTests.Application.DomainEventHandlers;

[TestSubject(typeof(OsagoExpiredHandler))]
public class OsagoExpiredHandlerShould
{
    private readonly OsagoExpiredDomainEvent _domainEvent = new(Guid.NewGuid());

    private readonly Osago _osago = Osago.Create(Guid.NewGuid(), "photoStorageBucketAndKet",
        DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));

    private readonly Mock<IOsagoRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTimeProvider = new();
    private readonly Mock<IMessageBus> _messageBusMock = new();

    private readonly OsagoExpiredHandler _handler;

    public OsagoExpiredHandlerShould()
    {
        _fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddYears(1).AddDays(1));
        _repositoryMock.Setup(x => x.GetByVehicleDocumentsId(It.IsAny<Guid>())).ReturnsAsync(_osago);
        _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(Result.Ok);
        _handler = new OsagoExpiredHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _messageBusMock.Object,
            _fakeTimeProvider);
    }

    [Fact]
    public async Task CommitUpdates()
    {
        // Arrange

        // Act
        await _handler.Handle(_domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task ThrowDataConsistencyViolationExceptionIfVehicleDocumentsNotFound()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetByVehicleDocumentsId(It.IsAny<Guid>())).ReturnsAsync(null as Osago);

        // Act
        Task Act()
        {
            return _handler.Handle(_domainEvent, TestContext.Current.CancellationToken);
        }

        // Assert
        await Assert.ThrowsAsync<DataConsistencyViolationException>(Act);
    }

    [Fact]
    public async Task ThrowExceptionIfCommitFailed()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(Result.Fail("error"));

        // Act
        Task Act()
        {
            return _handler.Handle(_domainEvent, TestContext.Current.CancellationToken);
        }

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(Act);
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