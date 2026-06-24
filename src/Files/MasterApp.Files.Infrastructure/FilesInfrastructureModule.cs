using MasterApp.Files.Infrastructure.Services;
using MasterApp.Files.Infrastructure.Settings;
using MasterApp.Files.UseCases.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minio;
using System.Net;

namespace MasterApp.Files.Infrastructure;

public static class FilesInfrastructureModule
{
    public static IServiceCollection AddFilesInfrastructure(this IServiceCollection services, IConfiguration Configuration)
    {
        var config = Configuration.GetRequiredSection(MinioSettings.Section);
        services.Configure<MinioSettings>(config);

        var minioSettings = config.Get<MinioSettings>() ?? throw new Exception("MinioSettings is null.");
        IWebProxy proxy = new WebProxy(minioSettings.ProxyAddress, minioSettings.ProxyPort);

        services.AddMinio(o =>
            o.WithEndpoint(minioSettings.Endpoint)
             .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey)
             .WithSSL(minioSettings.WithSSL)
             .WithProxy(proxy)
        );

        services.TryAddScoped<IFileUploader, MinioFileUploader>();
        services.TryAddScoped<IFileDownloader, MinioFileDownloader>();
        services.TryAddScoped<IFileRemover, MinioFileRemover>();

        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        services.RegisterFileDbContext(connectionString!);
        return services;
    }
}

