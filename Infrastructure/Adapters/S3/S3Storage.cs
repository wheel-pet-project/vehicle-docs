using System.Net;
using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Ports.S3;
using Domain.SharedKernel.Errors;
using Domain.SharedKernel.Exceptions.PublicExceptions;
using FluentResults;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Adapters.S3;

public class S3Storage(
    IAmazonS3 s3Client,
    IOptions<S3Options> options,
    ILogger<S3Storage> logger) : IS3Storage
{
    private readonly S3Options _s3Options = options.Value;

    public async Task<Result<(string frontPhotoBucketAndKey, string backPhotoBucketAndKey)>> SaveFrontAndBackPhotos(
        List<byte> frontPhotoBytes,
        List<byte> backPhotoBytes,
        DocumentType documentType)
    {
        if (frontPhotoBytes is null || backPhotoBytes is null) throw new ArgumentException("Photos are required");

        var currentBucket = GetCurrentBucket(documentType);
        var frontPhotoKey = GeneratePhotoKey();
        var backPhotoKey = GeneratePhotoKey();

        return await ProcessWithExceptionHandling(async () =>
        {
            var frontPhotoPutRequest = CreateRequest(currentBucket, frontPhotoKey, frontPhotoBytes);
            var backPhotoPutRequest = CreateRequest(currentBucket, backPhotoKey, backPhotoBytes);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var frontPhotoUploadTask = s3Client.PutObjectAsync(frontPhotoPutRequest, cts.Token);
            var backPhotoUploadTask = s3Client.PutObjectAsync(backPhotoPutRequest, cts.Token);

            await Task.WhenAll(frontPhotoUploadTask, backPhotoUploadTask);

            return Result.Ok(($"{currentBucket}/{frontPhotoKey}", $"{currentBucket}/{backPhotoKey}"));
        });
    }

    public async Task<Result<string>> SavePhoto(List<byte> photoBytes, DocumentType documentType)
    {
        if (photoBytes == null) throw new ArgumentException("Photo are required");

        var currentBucket = GetCurrentBucket(documentType);
        var photoKey = GeneratePhotoKey();

        return await ProcessWithExceptionHandling(async () =>
        {
            var putRequest = CreateRequest(currentBucket, photoKey, photoBytes);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await s3Client.PutObjectAsync(putRequest, cts.Token);

            return Result.Ok($"{currentBucket}/{photoKey}");
        });
    }

    private async Task<Result<T>> ProcessWithExceptionHandling<T>(Func<Task<Result<T>>> func)
    {
        try
        {
            return await func();
        }
        catch (AmazonS3Exception ex)
        {
            logger.LogError("Could not upload photos to S3 storage, exception : {ex}", ex);

            return ReturnСorrespondingResult<T>(ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning("Time-out for uploading photos has expired, exception: {ex}", ex);

            return ReturnTimeoutResult<T>();
        }
    }

    private string GetCurrentBucket(DocumentType documentType)
    {
        var rnd = new Random();
        var currentBucket = documentType switch
        {
            DocumentType.Sts => _s3Options.StsBuckets[rnd.Next(_s3Options.StsBuckets.Length)],
            DocumentType.Pts => _s3Options.PtsBuckets[rnd.Next(_s3Options.PtsBuckets.Length)],
            DocumentType.Osago => _s3Options.OsagoBuckets[rnd.Next(_s3Options.OsagoBuckets.Length)],
            _ => throw new ValueIsUnsupportedException($"{nameof(documentType)} is unknown")
        };

        return currentBucket;
    }

    private string GeneratePhotoKey()
    {
        return Guid.NewGuid().ToString();
    }

    private PutObjectRequest CreateRequest(string currentBucket, string key, List<byte> bytes)
    {
        var bytesArray = bytes.ToArray();

        return new PutObjectRequest
        {
            BucketName = currentBucket,
            Key = key,
            ContentType = "image/jpeg",
            InputStream = new MemoryStream(bytesArray),
            ChecksumSHA256 = Convert.ToBase64String(SHA256.HashData(bytesArray))
        };
    }

    private Result<T> ReturnTimeoutResult<T>()
    {
        return Result.Fail("Time-out for uploading photos has expired.");
    }

    private Result<T> ReturnСorrespondingResult<T>(AmazonS3Exception ex)
    {
        return ex.StatusCode > HttpStatusCode.InternalServerError
            ? Result.Fail(new ObjectStorageUnavailable("Object storage unavailable"))
            : Result.Fail("Could not upload photos to S3 storage");
    }
}