using FluentResults;
using Minio;
using Minio.DataModel.Args;

namespace MasterApp.Files.Infrastructure.Services;

internal abstract class MinioFileServiceBase(IMinioClient minioClient)
{
    public IMinioClient MinioClient { get; } = minioClient;

    protected async Task<Result> EnsureBucketAsync(string bucket, CancellationToken cancellationToken)
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(bucket);
            var isBucketExists = await MinioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);
            if (!isBucketExists)
            {
                var makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(bucket);
                await MinioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }
}



