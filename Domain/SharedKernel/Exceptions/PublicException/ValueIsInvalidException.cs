using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.PublicException;

[ExcludeFromCodeCoverage]
public class ValueIsInvalidException(string message) : PublicException(message);