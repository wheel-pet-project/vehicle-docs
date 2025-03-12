using Dapper;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.ValueObjects;
using FluentResults;
using MediatR;
using Npgsql;

namespace Application.UseCases.Queries.GetPtsByVehicleDocumentsId;

public class GetPtsByVehicleDocumentsIdQueryHandler(
    NpgsqlDataSource dataSource,
    string yandexS3StorageHost)
    : IRequestHandler<GetPtsByVehicleDocumentsIdQuery, Result<GetPtsByVehicleDocumentsIdQueryResponse>>
{
    public async Task<Result<GetPtsByVehicleDocumentsIdQueryResponse>> Handle(
        GetPtsByVehicleDocumentsIdQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var pts = await connection.QueryFirstOrDefaultAsync<PtsDapperModel>(Sql,
            new { Id = request.VehicleDocumentsId });
        if (pts == null ||
            pts.FrontPhotoStorageBucketAndKey == null ||
            pts.BackPhotoStorageBucketAndKey == null ||
            pts.YearOfManufacture == null ||
            pts.Color == null ||
            pts.Vin == null)
            return Result.Fail(new NotFound("Pts for vehicle doesn't exist"));

        var response = new GetPtsByVehicleDocumentsIdQueryResponse(
            $"{yandexS3StorageHost}/{pts.FrontPhotoStorageBucketAndKey}",
            $"{yandexS3StorageHost}/{pts.BackPhotoStorageBucketAndKey}",
            pts.YearOfManufacture.Value,
            Color.FromName(pts.Color),
            pts.Vin);

        return Result.Ok(response);
    }

    private record PtsDapperModel(
        string? FrontPhotoStorageBucketAndKey,
        string? BackPhotoStorageBucketAndKey,
        DateOnly? YearOfManufacture,
        string? Color,
        string? Vin);

    private const string Sql =
        """
        SELECT pts_front_photo_storage_bucket_and_key AS FrontPhotoStorageBucketAndKey,
               pts_back_photo_storage_bucket_and_key AS BackPhotoStorageBucketAndKey,
               pts_year_of_manufacture AS YearOfManufacture,
               pts_color AS Color,
               pts_vin AS Vin
        FROM vehicle_documents
        WHERE id = @Id
        """;
}