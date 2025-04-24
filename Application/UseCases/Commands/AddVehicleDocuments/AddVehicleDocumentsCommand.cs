using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddVehicleDocuments;

public record AddVehicleDocumentsCommand(Guid SagaId, Guid VehicleId) : IRequest<Result>;