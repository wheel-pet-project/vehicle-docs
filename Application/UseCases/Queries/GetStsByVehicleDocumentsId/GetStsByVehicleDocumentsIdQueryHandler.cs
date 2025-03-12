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
        if (sts == null || sts.FrontPhotoStorageBucketAndKey == null || sts.BackPhotoStorageBucketAndKey == null)
            return Result.Fail(new NotFound("Sts for vehicle doesn't exist"));

        var response = new GetStsByVehicleDocumentsIdQueryResponse(
            $"{yandexS3StorageHost}/{sts.FrontPhotoStorageBucketAndKey}",
            $"{yandexS3StorageHost}/{sts.BackPhotoStorageBucketAndKey}");

        return Result.Ok(response);
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