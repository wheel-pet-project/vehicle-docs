using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.PublicExceptions;

namespace Domain.SharedKernel.ValueObjects;

public class Vin : ValueObject
{
    private Vin()
    {
    }

    private Vin(string vinNumber) : this()
    {
        Number = vinNumber;
    }

    public string Number { get; } = null!;

    public static Vin Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ValueIsRequiredException($"{nameof(input)} vin number is required");

        var vinNumber = input.Trim();

        if (vinNumber.Length != 17)
            throw new ValueIsUnsupportedException($"{nameof(vinNumber)} must be 17 characters long");

        vinNumber = vinNumber.ToUpper();

        if (Regex.IsMatch(vinNumber, @"^\b[(A-H|J-N|P|R-Z|0-9)]{17}\b$") == false)
            throw new ValueIsInvalidException($"{nameof(vinNumber)} contains invalid characters");

        return new Vin(vinNumber);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
    }
}