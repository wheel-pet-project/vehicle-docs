using Application.Ports.ImageValidators;
using Application.Ports.Postgres;
using Application.Ports.S3;
using Application.UseCases.Commands.AddOsagoToVehicleDocuments;
using Domain.SharedKernel.Errors;
using Domain.VehicleDocumentsAggregate;
using FluentResults;
using Moq;
using Xunit;

namespace UnitTests.Application.UseCases;

public class AddOsagoHandlerShould
{
    private readonly VehicleDocuments _vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

    private readonly AddOsagoCommand _command = new(Guid.NewGuid(), [1, 2, 3], DateOnly.FromDateTime(DateTime.UtcNow),
        DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
    
    private readonly Mock<IVehicleDocumentsRepository> _vehicleDocumentsRepositoryMock = new();
    private readonly Mock<IOsagoRepository> _osagoRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IS3Storage> _s3StorageMock = new();
    private readonly Mock<IImageFormatValidator> _imageFormatValidatorMock = new();
    private readonly Mock<IImageSizeValidator> _imageSizeValidatorMock = new();
    
    private readonly AddOsagoHandler _handler;

    public AddOsagoHandlerShould()
    {
        _vehicleDocumentsRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(_vehicleDocuments);
        _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(Result.Ok);
        _s3StorageMock.Setup(x => x.SavePhoto(It.IsAny<List<byte>>())).ReturnsAsync(Result.Ok("photoKey"));
        _imageFormatValidatorMock.Setup(x => x.IsSupportedFormat(It.IsAny<List<byte>>())).Returns(true);
        _imageSizeValidatorMock.Setup(x => x.IsSupportedSize(It.IsAny<int>())).Returns(true);

        _handler = new AddOsagoHandler(_vehicleDocumentsRepositoryMock.Object, _osagoRepositoryMock.Object, 
            _unitOfWorkMock.Object, _s3StorageMock.Object, _imageFormatValidatorMock.Object, 
            _imageSizeValidatorMock.Object);
    }

    [Fact]
    public async Task ReturnSuccess()
    {
        // Arrange

        // Act
        var actual = await _handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(actual.IsSuccess);
    }

    [Fact]
    public async Task ReturnFailIfVehicleDocumentsNotFound()
    {
        // Arrange
        _vehicleDocumentsRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(null as VehicleDocuments);

        // Act
        var actual = await _handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(actual.IsSuccess);
        Assert.True(actual.Errors[0] is NotFound);
    }

    [Fact]
    public async Task ReturnFailIfImageFormatUnsupported()
    {
        // Arrange
        _imageFormatValidatorMock.Setup(x => x.IsSupportedFormat(It.IsAny<List<byte>>())).Returns(false);

        // Act
        var actual = await _handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(actual.IsSuccess);
    }

    [Fact]
    public async Task ReturnFailIfImageSizeUnsupported()
    {
        // Arrange
        _imageSizeValidatorMock.Setup(x => x.IsSupportedSize(It.IsAny<int>())).Returns(false);

        // Act
        var actual = await _handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(actual.IsSuccess);
    }

    [Fact]
    public async Task ReturnFailIfUploadToS3Failed()
    {
        // Arrange
        _s3StorageMock.Setup(x => x.SavePhoto(It.IsAny<List<byte>>())).ReturnsAsync(Result.Fail("error"));

        // Act
        var actual = await _handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(actual.IsSuccess);
    }

    [Fact]
    public async Task ReturnFailIfCommitFailed()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(Result.Fail("error"));

        // Act
        var actual = await _handler.Handle(_command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(actual.IsSuccess);
    }
}