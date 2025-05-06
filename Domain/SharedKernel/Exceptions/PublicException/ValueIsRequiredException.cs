using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.PublicException;

[ExcludeFromCodeCoverage]
public class ValueIsRequiredException(string message = "Parameter is required") : PublicException(message);