using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.PublicException;

[ExcludeFromCodeCoverage]
public class ValueIsUnsupportedException(string message = "Value is unsupported") : PublicException(message);