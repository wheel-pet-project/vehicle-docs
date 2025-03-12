using FluentResults;
using MediatR;

namespace Application.UseCases.Queries.GetVehicleDocumentsByVehicleId;

public record GetVehicleDocumentsByVehicleIdQuery(Guid VehicleId)
    : IRequest<Result<GetVehicleDocumentsByVehicleIdQueryResponse>>;