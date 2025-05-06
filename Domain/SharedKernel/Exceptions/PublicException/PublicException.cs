using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.PublicException;

[ExcludeFromCodeCoverage]
public class PublicException(string message) : Exception(message);