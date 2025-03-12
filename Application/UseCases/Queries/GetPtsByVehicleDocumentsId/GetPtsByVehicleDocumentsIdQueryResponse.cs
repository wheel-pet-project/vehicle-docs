using Domain.SharedKernel.ValueObjects;

namespace Application.UseCases.Queries.GetPtsByVehicleDocumentsId;

public record GetPtsByVehicleDocumentsIdQueryResponse(
    string FrontPhotoS3Url,
    string BackPhotoS3Url,
    DateOnly YearOfManufacture,
    Color Color,
    string Vin);