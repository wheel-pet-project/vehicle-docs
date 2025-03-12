using FluentResults;
using MediatR;

namespace Application.UseCases.Queries.GetOsagoByVehicleDocumentsId;

public record GetOsagoByVehicleDocumentsIdQuery(Guid VehicleDocumentsId)
    : IRequest<Result<GetOsagoByVehicleDocumentsIdQueryResponse>>;