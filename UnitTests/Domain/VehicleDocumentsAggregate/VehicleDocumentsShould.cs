using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.ValueObjects;
using Domain.VehicleDocumentsAggregate;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.VehicleDocumentsAggregate;

[TestSubject(typeof(VehicleDocuments))]
public class VehicleDocumentsShould
{
    private readonly Guid _vehicleId = Guid.NewGuid();
    
    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = VehicleDocuments.Create(_vehicleId);
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal(actual.VehicleId, _vehicleId);
        Assert.False(actual.Status.AddingCompleted);
        Assert.Null(actual.Pts);
        Assert.Null(actual.Sts);
        Assert.Null(actual.Osago);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfVehicleIdIsEmpty()
    {
        // Arrange

        // Act
        void Act() => VehicleDocuments.Create(Guid.Empty);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void AddPts()
    {
        // Arrange
        var pts = Pts.Create(new string('*', 10), new string('*', 10), DateOnly.FromDateTime(DateTime.UtcNow),
            Color.Beige, Vin.Create("SALYA2BN2KA791786"));
        var vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

        // Act
        vehicleDocuments.AddPts(pts);

        // Assert
        Assert.NotNull(vehicleDocuments.Pts);
        Assert.Equal(vehicleDocuments.Pts, pts);
    }
    
    [Fact]
    public void AddPtsThrowValueIsRequiredExceptionIfPtsIsNull()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

        // Act
        void Act() => vehicleDocuments.AddPts(null!);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
    
    [Fact]
    public void AddSts()
    {
        // Arrange
        var sts = Sts.Create(new string('*', 10), new string('*', 10));
        var vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

        // Act
        vehicleDocuments.AddSts(sts);

        // Assert
        Assert.NotNull(vehicleDocuments.Sts);
        Assert.Equal(vehicleDocuments.Sts, sts);
    }

    [Fact]
    public void AddStsThrowValueIsRequiredExceptionIfStsIsNull()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

        // Act
        void Act() => vehicleDocuments.AddSts(null!);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void AddOsago()
    {
        // Arrange
        var osago = Osago.Create(new string('*', 10),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        var vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

        // Act
        vehicleDocuments.AddOsago(osago);

        // Assert
        Assert.NotNull(vehicleDocuments.Osago);
        Assert.Equal(vehicleDocuments.Osago, osago);
    }

    [Fact]
    public void AddOsagoThrowValueIsRequiredExceptionIfOsagoIsNull()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(Guid.NewGuid());

        // Act
        void Act() => vehicleDocuments.AddOsago(null!);

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }
}