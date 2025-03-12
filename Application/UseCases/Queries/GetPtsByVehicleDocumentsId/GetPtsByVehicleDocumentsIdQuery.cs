using FluentResults;
using MediatR;

namespace Application.UseCases.Queries.GetPtsByVehicleDocumentsId;

public record GetPtsByVehicleDocumentsIdQuery(Guid VehicleDocumentsId)
    : IRequest<Result<GetPtsByVehicleDocumentsIdQueryResponse>>;