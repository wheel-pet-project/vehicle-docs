using Dapper;
using Domain.OsagoAggregate;
using Domain.SharedKernel.Errors;
using FluentResults;
using MediatR;
using Npgsql;

namespace Application.UseCases.Queries.GetOsagoByVehicleDocumentsId;

public class GetOsagoByVehicleDocumentsIdQueryHandler(
    NpgsqlDataSource dataSource,
    string yandexS3StorageHost)
    : IRequestHandler<GetOsagoByVehicleDocumentsIdQuery, Result<GetOsagoByVehicleDocumentsIdQueryResponse>>
{
    public async Task<Result<GetOsagoByVehicleDocumentsIdQueryResponse>> Handle(
        GetOsagoByVehicleDocumentsIdQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var osago = await connection.QueryFirstOrDefaultAsync<OsagoDapperModel>(Sql,
            new { request.VehicleDocumentsId });

        return osago == null
            ? Result.Fail(new NotFound("Osago for vehicle doesn't exist"))
            : Result.Ok(MapToResponse(osago));
    }

    private GetOsagoByVehicleDocumentsIdQueryResponse MapToResponse(OsagoDapperModel osago)
    {
        return new GetOsagoByVehicleDocumentsIdQueryResponse(
            $"{yandexS3StorageHost}/{osago.PhotoStorageBucketAndKey}",
            ExpiryStatus.FromId(osago.ExpiryStatusId),
            osago.DateOfIssue,
            osago.DateOfExpiry);
    }

    private record OsagoDapperModel(
        string PhotoStorageBucketAndKey,
        int ExpiryStatusId,
        DateOnly DateOfIssue,
        DateOnly DateOfExpiry);

    private const string Sql =
        """
        SELECT photo_storage_bucket_and_key AS PhotoStorageBucketAndKey,
               expiry_status_id AS ExpiryStatusId,
               date_of_issue AS DateOfIssue,
               date_of_expiry AS DateOfExpiry
        FROM osago
        WHERE vehicle_documents_id = @VehicleDocumentsId
        """;
}