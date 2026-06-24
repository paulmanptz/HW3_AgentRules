using FluentResults;
using MasterApp.Files.Infrastructure.Settings;
using MasterApp.Files.UseCases.Services;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace MasterApp.Files.Infrastructure.Services;

internal sealed class MinioFileDownloader(IMinioClient minioClient, IOptions<MinioSettings> minioOptions) : MinioFileServiceBase(minioClient), IFileDownloader
{
    public async Task<Result<IReadOnlyDictionary<string, string>>> GetAttachmentLinksAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken = default)
    {
        var ensureBucketResult = await EnsureBucketAsync(minioOptions.Value.AttachmentBucket.BucketName, cancellationToken);
        if (ensureBucketResult.IsFailed)
            return ensureBucketResult;

        var presignedGetObjectArgsBase = new PresignedGetObjectArgs()
            .WithBucket(minioOptions.Value.AttachmentBucket.BucketName)
            .WithExpiry(minioOptions.Value.AttachmentBucket.LinkExpirationInSeconds);

        var linkByName = new Dictionary<string, string>();
        foreach (var name in names.Distinct())
        {
            var presignedGetObjectArgs = presignedGetObjectArgsBase
                .WithObject(name);
            try
            {
                var link = await MinioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
                linkByName.Add(name, link);
            }
            catch (InvalidObjectNameException) { }
            catch (ObjectNotFoundException) { }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        return linkByName;
    }

    public async Task<Result<IReadOnlyDictionary<Guid, string>>> GetAttachmentLinksAsync(IReadOnlyCollection<Guid> fileIds, CancellationToken cancellationToken = default)
    {
        var ensureBucketResult = await EnsureBucketAsync(minioOptions.Value.AttachmentBucket.BucketName, cancellationToken);
        if (ensureBucketResult.IsFailed)
            return ensureBucketResult;

        var presignedGetObjectArgsBase = new PresignedGetObjectArgs()
            .WithBucket(minioOptions.Value.AttachmentBucket.BucketName)
            .WithExpiry(minioOptions.Value.AttachmentBucket.LinkExpirationInSeconds);

        var linkByName = new Dictionary<Guid, string>();
        foreach (var fileId in fileIds.Distinct())
        {
            var presignedGetObjectArgs = presignedGetObjectArgsBase
                .WithObject(fileId.ToString());
            try
            {
                var link = await MinioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
                linkByName.Add(fileId, link);
            }
            catch (InvalidObjectNameException) { }
            catch (ObjectNotFoundException) { }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        return linkByName;
    }
}


