using Microsoft.Extensions.DependencyInjection;

namespace MasterApp.Files.Controllers;

public static class FilesControllersModule
{
    public static IServiceCollection AddFilesControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(FilesControllersModule).Assembly)
            .AddControllersAsServices();
        return services;
    }
}

