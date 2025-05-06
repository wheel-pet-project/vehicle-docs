using Dapper;
using Domain.SharedKernel.Errors;
using FluentResults;
using MediatR;
using Npgsql;

namespace Application.UseCases.Queries.GetStsByVehicleDocumentsId;

public class GetStsByVehicleDocumentsIdQueryHandler(
    NpgsqlDataSource dataSource,
    string yandexS3StorageHost)
    : IRequestHandler<GetStsByVehicleDocumentsIdQuery, Result<GetStsByVehicleDocumentsIdQueryResponse>>
{
    public async Task<Result<GetStsByVehicleDocumentsIdQueryResponse>> Handle(
        GetStsByVehicleDocumentsIdQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var sts = await connection.QueryFirstOrDefaultAsync<StsDapperModel>(Sql,
            new { Id = request.VehicleDocumentsId });

        return IsEmptySts(sts) || sts == null
            ? Result.Fail(new NotFound("Sts for vehicle doesn't exist"))
            : Result.Ok(MapToResponse(sts));
    }

    private GetStsByVehicleDocumentsIdQueryResponse MapToResponse(StsDapperModel sts)
    {
        return new GetStsByVehicleDocumentsIdQueryResponse(
            $"{yandexS3StorageHost}/{sts.FrontPhotoStorageBucketAndKey}",
            $"{yandexS3StorageHost}/{sts.BackPhotoStorageBucketAndKey}");
    }

    private static bool IsEmptySts(StsDapperModel? sts)
    {
        return sts?.FrontPhotoStorageBucketAndKey == null || sts.BackPhotoStorageBucketAndKey == null;
    }

    private record StsDapperModel(
        string? FrontPhotoStorageBucketAndKey,
        string? BackPhotoStorageBucketAndKey);

    private const string Sql =
        """
        SELECT sts_front_photo_storage_bucket_and_key AS FrontPhotoStorageBucketAndKey,
               sts_back_photo_storage_bucket_and_key AS BackPhotoStorageBucketAndKey
        FROM vehicle_documents
        WHERE id = @Id
        """;
}