using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.InternalExceptions.AlreadyHaveThisState;

[ExcludeFromCodeCoverage]
public class SagaAlreadyHaveThisStateException(string message) : InternalException(message);