using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.AlreadyHaveThisState;

[ExcludeFromCodeCoverage]
public class SagaAlreadyHaveThisStateException(string message) : Exception(message);