using FluentResults;

namespace Application.Ports.S3;

public interface IS3Storage
{
    Task<Result<(string frontPhotoBucketAndKey, string backPhotoBucketAndKey)>> SaveFrontAndBackPhotos(
        List<byte> frontPhotoBytes,
        List<byte> backPhotoBytes,
        DocumentType documentType);

    Task<Result<string>> SavePhoto(List<byte> photoBytes, DocumentType documentType);
}

public enum DocumentType
{
    Sts = 0,
    Pts = 1,
    Osago = 2
}