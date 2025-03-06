using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.ArgumentException;

[ExcludeFromCodeCoverage]
public class ValueOutOfRangeException(string message = "Character out of range") : ArgumentException(message);