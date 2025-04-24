using Application.Ports.Postgres;
using Application.UseCases.Commands.AddVehicleDocuments;
using Domain.Services;
using FluentResults;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace UnitTests.Application.UseCases;

[TestSubject(typeof(AddVehicleDocumentsHandler))]
public class AddVehicleDocumentsHandlerShould
{
    private readonly AddVehicleDocumentsCommand _command = new(Guid.NewGuid(), Guid.NewGuid());

    private readonly Mock<ICreateVehicleDocumentsService> _mockCreateVehicleDocumentsServiceMock = new();
    private readonly Mock<IVehicleDocumentsRepository> _vehicleDocumentsRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly AddVehicleDocumentsHandler _handler;

    public AddVehicleDocumentsHandlerShould()
    {
        _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(Result.Ok);

        _handler = new AddVehicleDocumentsHandler(_mockCreateVehicleDocumentsServiceMock.Object,
            _vehicleDocumentsRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ReturnsSuccess()
    {
        // Arrange

        // Act
        var actual = await _handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(actual.IsSuccess);
    }

    [Fact]
    public async Task ReturnsFailIfCommitFailed()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(Result.Fail("fail"));

        // Act
        var actual = await _handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(actual.IsSuccess);
    }
}