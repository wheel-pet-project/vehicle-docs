using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.AlreadyHaveThisState;
using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.ValueObjects;
using Domain.VehicleDocumentsAggregate.DomainEvents;

namespace Domain.VehicleDocumentsAggregate;

public sealed class VehicleDocuments : Aggregate
{
    private VehicleDocuments()
    {
    }

    private VehicleDocuments(Guid vehicleId) : this()
    {
        Id = Guid.NewGuid();
        VehicleId = vehicleId;
        Status = Status.Create();
    }

    public Guid Id { get; }
    public Guid VehicleId { get; }
    public Status Status { get; } = null!;
    public Pts? Pts { get; private set; }
    public Sts? Sts { get; private set; }

    public void AddPts(Pts potentialPts)
    {
        if (potentialPts == null) throw new ValueIsRequiredException($"{nameof(potentialPts)} cannot be null");

        Pts = potentialPts;

        Status.MarkAsPtsAdded();
        if (Status.AddingCompleted) AddDomainEvent(new DocumentAddingCompletedDomainEvent(VehicleId));
    }

    public void AddSts(Sts potentialSts)
    {
        if (potentialSts == null) throw new ValueIsRequiredException($"{nameof(potentialSts)} cannot be null");

        Sts = potentialSts;

        Status.MarkAsStsAdded();
        if (Status.AddingCompleted) AddDomainEvent(new DocumentAddingCompletedDomainEvent(VehicleId));
    }

    public void MarkAsOsagoAdded()
    {
        if (Status.IsOsagoAdded) throw new AlreadyHaveThisStateException("Osago already added for this vehicle");

        Status.MarkAsOsagoAdded();
        if (Status.AddingCompleted) AddDomainEvent(new DocumentAddingCompletedDomainEvent(VehicleId));
    }

    public static VehicleDocuments Create(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(vehicleId)} cannot be empty");

        return new VehicleDocuments(vehicleId);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is VehicleDocuments documents && Id == documents.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Id);
    }
}