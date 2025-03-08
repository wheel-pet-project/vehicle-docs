using Domain.OsagoAggregate.DomainEvents;
using Domain.SharedKernel;
using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.Exceptions.DomainRulesViolationException;

namespace Domain.OsagoAggregate;

/// <summary>
/// ОСАГО 
/// </summary>
public sealed class Osago : Aggregate
{
    private Osago() {}

    private Osago(
        Guid vehicleDocumentsId, 
        string photoStorageBucketAndKey, 
        DateOnly dateOfIssue, 
        DateOnly dateOfExpiry) : this()
    {
        Id = Guid.NewGuid();
        VehicleDocumentsId = vehicleDocumentsId;
        PhotoStorageBucketAndKey = photoStorageBucketAndKey;
        ExpiryStatus = ExpiryStatus.NotExpired;
        DateOfIssue = dateOfIssue;
        DateOfExpiry = dateOfExpiry;
    }
    
    public Guid Id { get; }
    public Guid VehicleDocumentsId { get; }
    public string PhotoStorageBucketAndKey { get; } = null!;
    public ExpiryStatus ExpiryStatus { get; private set; } = null!;
    public DateOnly DateOfIssue { get; }
    public DateOnly DateOfExpiry  { get; }

    public bool IsExpired(TimeProvider timeProvider)
    {
        return timeProvider.GetUtcNow().UtcDateTime > DateOfExpiry.ToDateTime(new TimeOnly());
    }

    public void Expire(TimeProvider timeProvider)
    {
        if (timeProvider.GetUtcNow().UtcDateTime < DateOfExpiry.ToDateTime(new TimeOnly()))
            throw new DomainRulesViolationException($"{nameof(DateOfExpiry)} not come yet");
        
        ExpiryStatus = ExpiryStatus.Expired;
        
        AddDomainEvent(new OsagoExpiredDomainEvent(VehicleDocumentsId));
    }

    public static Osago Create(
        Guid vehicleDocumentsId, 
        string photoStorageBucketAndKey, 
        DateOnly dateOfIssue, 
        DateOnly dateOfExpiry)
    {
        if (vehicleDocumentsId == Guid.Empty) throw new ValueIsRequiredException(
            $"{nameof(vehicleDocumentsId)} cannot be empty");
        if (string.IsNullOrWhiteSpace(photoStorageBucketAndKey)) throw new ValueIsRequiredException(
            $"{nameof(photoStorageBucketAndKey)} cannot be null or whitespace");
        if (dateOfIssue == default) throw new ValueOutOfRangeException($"{nameof(dateOfIssue)} cannot be default");
        if (dateOfExpiry == default) throw new ValueOutOfRangeException($"{nameof(dateOfExpiry)} cannot be default");
        if (dateOfIssue > dateOfExpiry) throw new DomainRulesViolationException(
                $"{nameof(dateOfIssue)} cannot be greater than {nameof(dateOfExpiry)}");
        
        var osago = new Osago(vehicleDocumentsId, photoStorageBucketAndKey, dateOfIssue, dateOfExpiry);
        
        osago.AddDomainEvent(new OsagoAddedDomainEvent(osago.VehicleDocumentsId));
        
        return osago;
    }
}