namespace MasterApp.Files.Infrastructure.Settings;

internal sealed class MinioSettings
{
    public const string Section = "Minio";

    public required string Endpoint { get; init; }

    public required bool WithSSL { get; init; }

    public required string AccessKey { get; init; }

    public required string SecretKey { get; init; }

    public required AttachmentBucketSettings AttachmentBucket { get; init; }

    public required string ProxyAddress { get; init; }

    public required int ProxyPort { get; init; }
}

internal sealed class AttachmentBucketSettings
{
    public required string BucketName { get; init; }

    public required int LinkExpirationInSeconds { get; init; }
}
