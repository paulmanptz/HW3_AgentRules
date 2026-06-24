using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MasterApp.Files.Contracts.Implementations;

public static class FileContractsModule
{
    public static IServiceCollection AddFilesContracts(this IServiceCollection services)
    {
        services.TryAddScoped<IFileContract, MediatorFileContract>();
        services.TryAddScoped<IFileDetailsContract, MediatorFileDetailsContract>();
        return services;
    }
}

