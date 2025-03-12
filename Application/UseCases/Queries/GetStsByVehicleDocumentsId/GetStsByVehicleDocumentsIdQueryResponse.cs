namespace Application.UseCases.Queries.GetStsByVehicleDocumentsId;

public record GetStsByVehicleDocumentsIdQueryResponse(
    string FrontPhotoS3Url,
    string BackPhotoS3Url);