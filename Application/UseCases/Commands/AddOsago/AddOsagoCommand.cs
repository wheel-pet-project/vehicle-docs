using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddOsago;

public record AddOsagoCommand(
    Guid VehicleDocumentsId,
    List<byte> PhotoBytes,
    DateOnly DateOfIssue,
    DateOnly DateOfExpiry) : IRequest<Result>;