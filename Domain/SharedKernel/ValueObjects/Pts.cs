using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.SharedKernel.ValueObjects;

/// <summary>
/// Паспорт ТС 
/// </summary>
public class Pts : ValueObject
{
    private Pts() { }

    private Pts(
        string frontPhotoStorageBucketAndKey,
        string backPhotoStorageBucketAndKey,
        DateOnly yearOfManufacture,
        Color color,
        Vin vin) : this()
    {
        FrontPhotoStorageBucketAndKey = frontPhotoStorageBucketAndKey;
        BackPhotoStorageBucketAndKey = backPhotoStorageBucketAndKey;
        YearOfManufacture = yearOfManufacture;
        Color = color;
        Vin = vin;
    }
    
    public string FrontPhotoStorageBucketAndKey { get; } = null!;
    public string BackPhotoStorageBucketAndKey { get; } = null!;
    public DateOnly YearOfManufacture { get; }
    public Color Color { get; } = null!;
    public Vin Vin { get; } = null!;

    public static Pts Create(
        string frontPhotoStorageBucketAndKey,
        string backPhotoStorageBucketAndKey,
        DateOnly yearOfManufacture,
        Color color,
        Vin vin)
    {
        if (string.IsNullOrWhiteSpace(frontPhotoStorageBucketAndKey))
            throw new ValueIsRequiredException($"{nameof(frontPhotoStorageBucketAndKey)} cannot be null or whitespace");
        if (string.IsNullOrWhiteSpace(backPhotoStorageBucketAndKey))
            throw new ValueIsRequiredException($"{nameof(backPhotoStorageBucketAndKey)} cannot be null or whitespace");
        if (yearOfManufacture == default)
            throw new ValueOutOfRangeException($"{nameof(yearOfManufacture)} cannot be default value");
        if (color == null) throw new ValueIsRequiredException($"{nameof(color)} cannot be null");
        if (vin == null) throw new ValueIsRequiredException($"{nameof(vin)} cannot be null");

        return new Pts(frontPhotoStorageBucketAndKey, backPhotoStorageBucketAndKey, yearOfManufacture, color, vin);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FrontPhotoStorageBucketAndKey;
        yield return BackPhotoStorageBucketAndKey;
        yield return YearOfManufacture;
        yield return Color;
        yield return Vin;
    }
}