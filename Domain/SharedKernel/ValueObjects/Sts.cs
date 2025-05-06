using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.PublicException;

namespace Domain.SharedKernel.ValueObjects;

/// <summary>
/// Свидетельство о регистрации ТС 
/// </summary>
public class Sts : ValueObject
{
    private Sts()
    {
    }

    private Sts(string frontPhotoStorageBucketAndKey, string backPhotoStorageBucketAndKey) : this()
    {
        FrontPhotoStorageBucketAndKey = frontPhotoStorageBucketAndKey;
        BackPhotoStorageBucketAndKey = backPhotoStorageBucketAndKey;
    }

    public string FrontPhotoStorageBucketAndKey { get; } = null!;
    public string BackPhotoStorageBucketAndKey { get; } = null!;

    public static Sts Create(string frontPhotoStorageBucketAndKey, string backPhotoStorageBucketAndKey)
    {
        if (string.IsNullOrWhiteSpace(frontPhotoStorageBucketAndKey))
            throw new ValueIsRequiredException($"'{nameof(frontPhotoStorageBucketAndKey)}' cannot be null or empty");
        if (string.IsNullOrWhiteSpace(backPhotoStorageBucketAndKey))
            throw new ValueIsRequiredException($"{nameof(backPhotoStorageBucketAndKey)} cannot be null or empty");

        return new Sts(frontPhotoStorageBucketAndKey, backPhotoStorageBucketAndKey);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FrontPhotoStorageBucketAndKey;
        yield return BackPhotoStorageBucketAndKey;
    }
}