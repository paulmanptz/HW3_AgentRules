using FluentResults;
using MasterApp.Files.Infrastructure.Settings;
using MasterApp.Files.UseCases.Services;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace MasterApp.Files.Infrastructure.Services;

internal sealed class MinioFileRemover(IMinioClient minioClient, IOptions<MinioSettings> minioOptions) : MinioFileServiceBase(minioClient), IFileRemover
{
    public async Task<Result> RemoveAttachmentAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken = default)
    {
        var ensureBucketResult = await EnsureBucketAsync(minioOptions.Value.AttachmentBucket.BucketName, cancellationToken);
        if (ensureBucketResult.IsFailed)
            return ensureBucketResult;

        var removeObjectsResult = await RemoveObjectsAsync(names, minioOptions.Value.AttachmentBucket.BucketName, cancellationToken);
        if (removeObjectsResult.IsFailed)
            return removeObjectsResult;

        return Result.Ok();
    }

    private async Task<Result> RemoveObjectsAsync(
        IReadOnlyCollection<string> names,
        string bucket,
        CancellationToken cancellationToken)
    {
        var removeObjectArgs = new RemoveObjectsArgs()
            .WithBucket(bucket)
            .WithObjects(names.ToArray());

        try
        {
            var deleteErrors = await MinioClient.RemoveObjectsAsync(removeObjectArgs, cancellationToken);
            if (deleteErrors.Count > 0)
            {
                var errors = deleteErrors.Select(error => new Error($"Bucket = '{error.BucketName}', Message = '{error.Message}'"));
                return Result.Fail(errors);
            }
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }

        return Result.Ok();
    }
}



