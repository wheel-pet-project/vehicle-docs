using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddVehicleDocuments;

public record AddVehicleDocumentsCommand(Guid VehicleId) : IRequest<Result>;