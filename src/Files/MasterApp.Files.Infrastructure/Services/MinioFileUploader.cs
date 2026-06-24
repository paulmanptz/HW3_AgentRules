using FluentResults;
using MasterApp.Files.Infrastructure.Settings;
using MasterApp.Files.UseCases.Services;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace MasterApp.Files.Infrastructure.Services;

internal sealed class MinioFileUploader(IMinioClient minioClient, IOptions<MinioSettings> minioOptions) : MinioFileServiceBase(minioClient), IFileUploader
{
    public async Task<Result> UploadAttachmentAsync(Stream stream, string name, string contentType, long size, CancellationToken cancellationToken = default)
    {
        var ensureBucketResult = await EnsureBucketAsync(minioOptions.Value.AttachmentBucket.BucketName, cancellationToken);
        if (ensureBucketResult.IsFailed)
            return ensureBucketResult;

        var putObjectResult = await PutObjectAsync(stream, name, contentType, size, minioOptions.Value.AttachmentBucket.BucketName, cancellationToken);
        if (putObjectResult.IsFailed)
            return putObjectResult;

        return Result.Ok();
    }

    private async Task<Result> PutObjectAsync(
        Stream stream,
        string name,
        string contentType,
        long size,
        string bucket,
        CancellationToken cancellationToken)
    {
        try
        {
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(name)
                .WithStreamData(stream)
                .WithContentType(contentType)
                .WithObjectSize(size);

            await MinioClient.PutObjectAsync(putObjectArgs, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }
}



