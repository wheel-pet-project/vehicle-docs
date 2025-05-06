using Application.Ports.ImageValidators;
using Application.Ports.Postgres;
using Application.Ports.S3;
using Domain.OsagoAggregate;
using Domain.SharedKernel.Errors;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.AddOsago;

public class AddOsagoHandler(
    IVehicleDocumentsRepository vehicleDocumentsRepository,
    IOsagoRepository osagoRepository,
    IUnitOfWork unitOfWork,
    IS3Storage s3Storage,
    IImageValidator imageValidator) : IRequestHandler<AddOsagoCommand, Result>
{
    public async Task<Result> Handle(AddOsagoCommand command, CancellationToken cancellationToken)
    {
        var vehicleDocuments = await vehicleDocumentsRepository.GetById(command.VehicleDocumentsId);
        if (vehicleDocuments == null) return Result.Fail(new NotFound("Vehicle documents not found"));

        var validatingResult = ValidatePhotos(command);
        if (validatingResult.IsFailed) return validatingResult;

        var uploadingToS3Result = await s3Storage.SavePhoto(command.PhotoBytes, DocumentType.Osago);
        if (uploadingToS3Result.IsFailed) return Result.Fail(uploadingToS3Result.Errors);
        var photoBucketAndKey = uploadingToS3Result.Value;

        var osago = Osago.Create(command.VehicleDocumentsId, photoBucketAndKey, command.DateOfIssue,
            command.DateOfExpiry);

        await osagoRepository.Add(osago);

        return await unitOfWork.Commit();
    }

    private Result ValidatePhotos(AddOsagoCommand command)
    {
        if (imageValidator.IsSupportedFormat(command.PhotoBytes) is false)
            return Result.Fail("Image format is not supported");
        if (imageValidator.IsSupportedSize(command.PhotoBytes.Count) is false)
            return Result.Fail("Image size is too large");

        return Result.Ok();
    }
}