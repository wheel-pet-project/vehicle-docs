using Dapper;
using Domain.SharedKernel.Errors;
using Domain.VehicleDocumentsAggregate;
using FluentResults;
using MediatR;
using Npgsql;

namespace Application.UseCases.Queries.GetVehicleDocumentsByVehicleId;

public class GetVehicleDocumentsByVehicleIdQueryHandler(NpgsqlDataSource dataSource)
    : IRequestHandler<GetVehicleDocumentsByVehicleIdQuery, Result<GetVehicleDocumentsByVehicleIdQueryResponse>>
{
    public async Task<Result<GetVehicleDocumentsByVehicleIdQueryResponse>> Handle(
        GetVehicleDocumentsByVehicleIdQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(Sql, new { request.VehicleId }, cancellationToken: cancellationToken);
        var vehicleDocuments = await connection.QueryFirstOrDefaultAsync<VehicleDocumentsDapperModel>(command);

        return vehicleDocuments == null
            ? Result.Fail(new NotFound("Documents for vehicle doesn't exist"))
            : Result.Ok(MapToResponse(vehicleDocuments));
    }

    private GetVehicleDocumentsByVehicleIdQueryResponse MapToResponse(VehicleDocumentsDapperModel vehicleDocuments)
    {
        return new GetVehicleDocumentsByVehicleIdQueryResponse(vehicleDocuments.Id, vehicleDocuments.VehicleId,
            Status.FromValues(vehicleDocuments.IsPtsAdded, vehicleDocuments.IsStsAdded, vehicleDocuments.IsOsagoAdded));
    }

    private record VehicleDocumentsDapperModel(
        Guid Id,
        Guid VehicleId,
        bool IsPtsAdded,
        bool IsStsAdded,
        bool IsOsagoAdded);

    private const string Sql =
        """
        SELECT id AS Id,
        vehicle_id AS VehicleId,
        is_pts_added AS IsPtsAdded,
        is_sts_added AS IsStsAdded,
        is_osago_added AS IsOsagoAdded
        FROM vehicle_documents
        WHERE vehicle_id = @VehicleId
        """;
}