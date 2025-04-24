using System.Diagnostics.CodeAnalysis;
using FluentResults;

namespace Domain.SharedKernel.Errors;

[ExcludeFromCodeCoverage]
public class CommitFail(string message, Exception exception) : Error(message)
{
    public Exception Exception = exception;
}