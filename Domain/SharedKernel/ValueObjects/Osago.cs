using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.ArgumentException;
using Domain.SharedKernel.Exceptions.DomainRulesViolationException;

namespace Domain.SharedKernel.ValueObjects;

/// <summary>
/// ОСАГО 
/// </summary>
public sealed class Osago : ValueObject
{
    private Osago() {}

    private Osago(string photoStorageBucketAndKey, DateOnly dateOfIssue, DateOnly dateOfExpiry) : this()
    {
        PhotoStorageBucketAndKey = photoStorageBucketAndKey;
        DateOfIssue = dateOfIssue;
        DateOfExpiry = dateOfExpiry;
    }

    public string PhotoStorageBucketAndKey { get; } = null!;
    public DateOnly DateOfIssue { get; }
    public DateOnly DateOfExpiry  { get; }

    public static Osago Create(string photoStorageBucketAndKey, DateOnly dateOfIssue, DateOnly dateOfExpiry)
    {
        if (string.IsNullOrWhiteSpace(photoStorageBucketAndKey)) throw new ValueIsRequiredException(
            $"{nameof(photoStorageBucketAndKey)} cannot be null or whitespace");
        if (dateOfIssue == default) throw new ValueOutOfRangeException($"{nameof(dateOfIssue)} cannot be default");
        if (dateOfExpiry == default) throw new ValueOutOfRangeException($"{nameof(dateOfExpiry)} cannot be default");
        if (dateOfIssue > dateOfExpiry) throw new DomainRulesViolationException(
                $"{nameof(dateOfIssue)} cannot be greater than {nameof(dateOfExpiry)}");

        return new Osago(photoStorageBucketAndKey, dateOfIssue, dateOfExpiry);
    } 
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PhotoStorageBucketAndKey;
        yield return DateOfIssue;
        yield return DateOfExpiry;
    }
}