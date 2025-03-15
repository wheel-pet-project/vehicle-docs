using Domain.VehicleDocumentsAggregate;

namespace Application.UseCases.Queries.GetVehicleDocumentsByVehicleId;

public record GetVehicleDocumentsByVehicleIdQueryResponse(
    Guid VehicleDocumentsId,
    Guid VehicleId,
    Status Status);