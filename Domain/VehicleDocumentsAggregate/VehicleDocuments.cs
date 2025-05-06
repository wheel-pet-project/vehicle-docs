using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.InternalExceptions.AlreadyHaveThisState;
using Domain.SharedKernel.Exceptions.PublicExceptions;
using Domain.SharedKernel.ValueObjects;
using Domain.VehicleDocumentsAggregate.DomainEvents;

namespace Domain.VehicleDocumentsAggregate;

public sealed class VehicleDocuments : Aggregate
{
    private VehicleDocuments()
    {
    }

    private VehicleDocuments(Guid sagaId, Guid vehicleId) : this()
    {
        Id = Guid.NewGuid();
        SagaId = sagaId;
        VehicleId = vehicleId;
        Status = Status.Create();
    }

    public Guid Id { get; }
    public Guid SagaId { get; }
    public Guid VehicleId { get; }
    public Status Status { get; } = null!;
    public Pts? Pts { get; private set; }
    public Sts? Sts { get; private set; }

    public void AddPts(Pts potentialPts)
    {
        if (potentialPts == null) throw new ValueIsRequiredException($"{nameof(potentialPts)} cannot be null");

        Pts = potentialPts;

        Status.MarkAsPtsAdded();

        AddAddingCompletedDomainEventIfNecessary();
    }

    public void AddSts(Sts potentialSts)
    {
        if (potentialSts == null) throw new ValueIsRequiredException($"{nameof(potentialSts)} cannot be null");

        Sts = potentialSts;

        Status.MarkAsStsAdded();

        AddAddingCompletedDomainEventIfNecessary();
    }

    public void MarkAsOsagoAdded()
    {
        if (Status.IsOsagoAdded) throw new AlreadyHaveThisStateException("Osago already added for this vehicle");

        Status.MarkAsOsagoAdded();

        AddAddingCompletedDomainEventIfNecessary();
    }

    private void AddAddingCompletedDomainEventIfNecessary()
    {
        if (Status.AddingCompleted) AddDomainEvent(new DocumentAddingCompletedDomainEvent(SagaId, VehicleId));
    }

    public static VehicleDocuments Create(Guid sagaId, Guid vehicleId)
    {
        if (sagaId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(sagaId)} cannot be empty");
        if (vehicleId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(vehicleId)} cannot be empty");

        return new VehicleDocuments(sagaId, vehicleId);
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