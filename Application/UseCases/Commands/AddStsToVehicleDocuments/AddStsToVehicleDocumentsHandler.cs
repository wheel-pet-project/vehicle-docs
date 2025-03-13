using Application.Ports.ImageValidators;
using Application.Ports.Postgres;
using Application.Ports.S3;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.ValueObjects;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddStsToVehicleDocuments;

public class AddStsToVehicleDocumentsHandler(
    IVehicleDocumentsRepository vehicleDocumentsRepository,
    IUnitOfWork unitOfWork,
    IS3Storage s3Storage,
    IImageFormatValidator formatValidator,
    IImageSizeValidator sizeValidator) : IRequestHandler<AddStsToVehicleDocumentsCommand, Result>
{
    public async Task<Result> Handle(AddStsToVehicleDocumentsCommand command, CancellationToken _)
    {
        var vehicleDocuments = await vehicleDocumentsRepository.GetById(command.VehicleDocumentsId);
        if (vehicleDocuments == null) return Result.Fail(new NotFound("Vehicle documents not found"));

        if (formatValidator.IsSupportedFormat(command.FrontPhotoBytes) is false ||
            formatValidator.IsSupportedFormat(command.BackPhotoBytes) is false)
            return Result.Fail("Image format is not supported");

        if (sizeValidator.IsSupportedSize(command.FrontPhotoBytes.Count) is false ||
            sizeValidator.IsSupportedSize(command.BackPhotoBytes.Count) is false)
            return Result.Fail("Image size is too large");

        var uploadingToS3Result =
            await s3Storage.SavePhotos(command.FrontPhotoBytes, command.BackPhotoBytes, DocumentType.Sts);
        if (uploadingToS3Result.IsFailed) return Result.Fail(uploadingToS3Result.Errors);
        var (frontPhotoBucketAndKey, backPhotoBucketAndKey) = uploadingToS3Result.Value;

        var sts = Sts.Create(frontPhotoBucketAndKey, backPhotoBucketAndKey);

        vehicleDocuments.AddSts(sts);

        vehicleDocumentsRepository.Update(vehicleDocuments);

        return await unitOfWork.Commit();
    }
}