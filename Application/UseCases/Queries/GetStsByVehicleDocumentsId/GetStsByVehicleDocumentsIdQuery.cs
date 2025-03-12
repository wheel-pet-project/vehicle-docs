using FluentResults;
using MediatR;

namespace Application.UseCases.Queries.GetStsByVehicleDocumentsId;

public record GetStsByVehicleDocumentsIdQuery(Guid VehicleDocumentsId)
    : IRequest<Result<GetStsByVehicleDocumentsIdQueryResponse>>;