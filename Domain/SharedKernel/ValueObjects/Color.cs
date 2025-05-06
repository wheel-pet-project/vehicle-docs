using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.PublicException;

namespace Domain.SharedKernel.ValueObjects;

public sealed class Color : ValueObject
{
    public static readonly Color White = new(nameof(White).ToLowerInvariant());
    public static readonly Color Grey = new(nameof(Grey).ToLowerInvariant());
    public static readonly Color Black = new(nameof(Black).ToLowerInvariant());
    public static readonly Color Blue = new(nameof(Blue).ToLowerInvariant());
    public static readonly Color Red = new(nameof(Red).ToLowerInvariant());
    public static readonly Color Yellow = new(nameof(Yellow).ToLowerInvariant());
    public static readonly Color Orange = new(nameof(Orange).ToLowerInvariant());
    public static readonly Color Green = new(nameof(Green).ToLowerInvariant());
    public static readonly Color Beige = new(nameof(Beige).ToLowerInvariant());

    private Color()
    {
    }

    private Color(string color) : this()
    {
        Name = color;
    }

    public string Name { get; } = null!;

    public static IEnumerable<Color> All()
    {
        return
        [
            White,
            Grey,
            Black,
            Blue,
            Red,
            Yellow,
            Orange,
            Green,
            Beige
        ];
    }

    public static Color FromName(string name)
    {
        var color = All().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (color == null)
            throw new ValueIsUnsupportedException(
                $"{nameof(name)} unknown color or null, color must be one of: {string.Join(' ', All())}");

        return color;
    }

    public override string ToString()
    {
        return Name.ToLowerInvariant();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}