using Domain.Services;
using Domain.SharedKernel.Exceptions.InternalExceptions.AlreadyHaveThisState;
using Domain.VehicleDocumentsAggregate;
using Xunit;

namespace UnitTests.Domain.Services;

public class CreateVehicleDocumentsServiceShould
{
    private readonly CreateVehicleDocumentsService _service = new();
    private readonly VehicleDocuments _vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void CreateVehicleDocuments()
    {
        // Arrange

        // Act
        var actual = _service.Create(null, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.NotNull(actual);
    }

    [Fact]
    public void ThrowAlreadyHaveThisStateExceptionIfDocumentAlreadyExists()
    {
        // Arrange

        // Act
        void Act() => _service.Create(_vehicleDocuments, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Throws<AlreadyHaveThisStateException>(Act);
    }
}