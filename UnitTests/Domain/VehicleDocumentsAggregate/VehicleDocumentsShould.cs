using Domain.SharedKernel.Exceptions.AlreadyHaveThisState;
using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.ValueObjects;
using Domain.VehicleDocumentsAggregate;
using JetBrains.Annotations;
using Xunit;

namespace UnitTests.Domain.VehicleDocumentsAggregate;

[TestSubject(typeof(VehicleDocuments))]
public class VehicleDocumentsShould
{
    private readonly Guid _sagaId = Guid.NewGuid();
    private readonly Guid _vehicleId = Guid.NewGuid();
    private readonly Sts _sts = Sts.Create(new string('*', 10), new string('*', 10));

    private readonly Pts _pts = Pts.Create(new string('*', 10), new string('*', 10),
        DateOnly.FromDateTime(DateTime.UtcNow),
        Color.Beige, Vin.Create("SALYA2BN2KA791786"));

    [Fact]
    public void CreateNewInstanceWithCorrectValues()
    {
        // Arrange

        // Act
        var actual = VehicleDocuments.Create(_sagaId, _vehicleId);

        // Assert
        Assert.NotNull(actual);
        Assert.NotEqual(Guid.Empty, actual.Id);
        Assert.Equal(actual.VehicleId, _vehicleId);
        Assert.False(actual.Status.AddingCompleted);
        Assert.Null(actual.Pts);
        Assert.Null(actual.Sts);
    }
    
    [Fact]
    public void ThrowValueIsRequiredExceptionIfSagaIdIsEmpty()
    {
        // Arrange

        // Act
        void Act()
        {
            VehicleDocuments.Create(Guid.Empty, _vehicleId);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void ThrowValueIsRequiredExceptionIfVehicleIdIsEmpty()
    {
        // Arrange

        // Act
        void Act()
        {
            VehicleDocuments.Create(_sagaId, Guid.Empty);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void AddPts()
    {
        // Arrange
        var pts = Pts.Create(new string('*', 10), new string('*', 10), DateOnly.FromDateTime(DateTime.UtcNow),
            Color.Beige, Vin.Create("SALYA2BN2KA791786"));
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);

        // Act
        vehicleDocuments.AddPts(pts);

        // Assert
        Assert.NotNull(vehicleDocuments.Pts);
        Assert.Equal(pts, vehicleDocuments.Pts);
    }

    [Fact]
    public void AddPtsChangeStatus()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);

        // Act
        vehicleDocuments.AddPts(_pts);

        // Assert
        Assert.True(vehicleDocuments.Status.IsPtsAdded);
    }

    [Fact]
    public void AddPtsAddDomainEventIfAllDocumentsAreAdded()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);
        vehicleDocuments.AddSts(_sts);
        vehicleDocuments.MarkAsOsagoAdded();
        vehicleDocuments.ClearDomainEvents();

        // Act
        vehicleDocuments.AddPts(_pts);

        // Assert
        Assert.NotEmpty(vehicleDocuments.DomainEvents);
    }

    [Fact]
    public void AddPtsThrowValueIsRequiredExceptionIfPtsIsNull()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);

        // Act
        void Act()
        {
            vehicleDocuments.AddPts(null!);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void AddSts()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);

        // Act
        vehicleDocuments.AddSts(_sts);

        // Assert
        Assert.NotNull(vehicleDocuments.Sts);
        Assert.Equal(_sts, vehicleDocuments.Sts);
    }

    [Fact]
    public void AddStsChangeStatus()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);

        // Act
        vehicleDocuments.AddSts(_sts);

        // Assert
        Assert.True(vehicleDocuments.Status.IsStsAdded);
    }

    [Fact]
    public void AddStsAddDomainEvent()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);
        vehicleDocuments.AddPts(_pts);
        vehicleDocuments.MarkAsOsagoAdded();
        vehicleDocuments.ClearDomainEvents();

        // Act
        vehicleDocuments.AddSts(_sts);

        // Assert
        Assert.NotEmpty(vehicleDocuments.DomainEvents);
    }

    [Fact]
    public void AddStsThrowValueIsRequiredExceptionIfStsIsNull()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);

        // Act
        void Act()
        {
            vehicleDocuments.AddSts(null!);
        }

        // Assert
        Assert.Throws<ValueIsRequiredException>(Act);
    }

    [Fact]
    public void MarkAsOsagoAddedChangeStatus()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);

        // Act
        vehicleDocuments.MarkAsOsagoAdded();

        // Assert
        Assert.True(vehicleDocuments.Status.IsOsagoAdded);
    }

    [Fact]
    public void MarkAsOsagoAddedAddDomainEvent()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);
        vehicleDocuments.AddPts(_pts);
        vehicleDocuments.AddSts(_sts);
        vehicleDocuments.ClearDomainEvents();

        // Act
        vehicleDocuments.MarkAsOsagoAdded();

        // Assert
        Assert.NotEmpty(vehicleDocuments.DomainEvents);
    }

    [Fact]
    public void MarkAsOsagoAddedThrowAlreadyHaveThisStateExceptionIfOsagoAlreadyAdded()
    {
        // Arrange
        var vehicleDocuments = VehicleDocuments.Create(_sagaId, _vehicleId);
        vehicleDocuments.AddPts(_pts);
        vehicleDocuments.AddSts(_sts);
        vehicleDocuments.MarkAsOsagoAdded();

        // Act
        void Act()
        {
            vehicleDocuments.MarkAsOsagoAdded();
        }

        // Assert
        Assert.Throws<AlreadyHaveThisStateException>(Act);
    }
}