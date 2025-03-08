using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddOsagoToVehicleDocuments;

public record AddOsagoCommand(
    Guid VehicleDocumentsId,
    List<byte> PhotoBytes,
    DateOnly DateOfIssue,
    DateOnly DateOfExpiry) : IRequest<Result>;