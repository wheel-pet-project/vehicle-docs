using Application.DomainEventHandlers;
using Application.Ports.Postgres;
using Domain.OsagoAggregate.DomainEvents;
using Domain.SharedKernel.Exceptions.InternalExceptions;
using Domain.VehicleDocumentsAggregate;
using FluentResults;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.DomainEventHandlers;

[TestSubject(typeof(OsagoAddedHandler))]
public class OsagoAddedHandlerShould
{
    private readonly OsagoAddedDomainEvent _domainEvent = new(Guid.NewGuid());

    private readonly VehicleDocuments _vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid(), Guid.NewGuid());

    private readonly Mock<IVehicleDocumentsRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly OsagoAddedHandler _handler;

    public OsagoAddedHandlerShould()
    {
        _repositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(_vehicleDocuments);
        _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(Result.Ok);
        _handler = new OsagoAddedHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
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
        _repositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(null as VehicleDocuments);

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
}