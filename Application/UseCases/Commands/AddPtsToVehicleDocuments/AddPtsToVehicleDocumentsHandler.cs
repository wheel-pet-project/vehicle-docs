using Application.Ports.ImageValidators;
using Application.Ports.Postgres;
using Application.Ports.S3;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.ValueObjects;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddPtsToVehicleDocuments;

public class AddPtsToVehicleDocumentsHandler(
    IVehicleDocumentsRepository vehicleDocumentsRepository,
    IUnitOfWork unitOfWork,
    IS3Storage s3Storage,
    IImageFormatValidator formatValidator,
    IImageSizeValidator sizeValidator) : IRequestHandler<AddPtsToVehicleDocumentsCommand, Result>
{
    public async Task<Result> Handle(AddPtsToVehicleDocumentsCommand command, CancellationToken _)
    {
        var vehicleDocuments = await vehicleDocumentsRepository.GetById(command.VehicleDocumentsId);
        if (vehicleDocuments == null) return Result.Fail(new NotFound("Vehicle documents not found"));

        if (formatValidator.IsSupportedFormat(command.FrontPhotoBytes) is false ||
            formatValidator.IsSupportedFormat(command.BackPhotoBytes) is false)
            return Result.Fail("Image format is not supported");

        if (sizeValidator.IsSupportedSize(command.FrontPhotoBytes.Count) is false ||
            sizeValidator.IsSupportedSize(command.BackPhotoBytes.Count) is false)
            return Result.Fail("Image size is too large");

        var vin = Vin.Create(command.Vin);

        var uploadingToS3Result = await s3Storage.SavePhotos(command.FrontPhotoBytes, command.BackPhotoBytes);
        if (uploadingToS3Result.IsFailed) return Result.Fail(uploadingToS3Result.Errors);
        var (frontPhotoBucketAndKey, backPhotoBucketAndKey) = uploadingToS3Result.Value;

        var pts = Pts.Create(
            frontPhotoBucketAndKey,
            backPhotoBucketAndKey,
            command.YearOfManufacture,
            command.Color,
            vin);

        vehicleDocuments.AddPts(pts);

        vehicleDocumentsRepository.Update(vehicleDocuments);

        return await unitOfWork.Commit();
    }
}