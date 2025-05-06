using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.InternalExceptions.AlreadyHaveThisState;

[ExcludeFromCodeCoverage]
public class AlreadyHaveThisStateException(string message) : InternalException(message);