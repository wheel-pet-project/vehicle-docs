using Domain.SharedKernel.ValueObjects;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddPtsToVehicleDocuments;

public record AddPtsToVehicleDocumentsCommand(
    Guid VehicleDocumentsId,
    List<byte> FrontPhotoBytes,
    List<byte> BackPhotoBytes,
    DateOnly YearOfManufacture, 
    Color Color, 
    string Vin) : IRequest<Result>;