using Domain.OsagoAggregate;

namespace Application.UseCases.Queries.GetOsagoByVehicleDocumentsId;

public record GetOsagoByVehicleDocumentsIdQueryResponse(
    string PhotoS3Url,
    ExpiryStatus ExpiryStatus,
    DateOnly DateOfIssue,
    DateOnly DateOfExpiry);