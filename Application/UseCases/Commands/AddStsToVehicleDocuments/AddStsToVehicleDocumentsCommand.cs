using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddStsToVehicleDocuments;

public record AddStsToVehicleDocumentsCommand(
    Guid VehicleDocumentsId,
    List<byte> FrontPhotoBytes,
    List<byte> BackPhotoBytes) : IRequest<Result>;