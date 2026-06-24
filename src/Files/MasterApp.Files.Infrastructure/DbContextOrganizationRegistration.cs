using MasterApp.Files.Infrastructure.Database;
using MasterApp.Files.UseCases;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace MasterApp.Files.Infrastructure;

using Microsoft.EntityFrameworkCore;

internal static class DbContextOrganizationRegistration
{
    internal static IServiceCollection RegisterFileDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FileDbContext>(options =>
            options.UseNpgsql(connectionString, o =>
            {
                o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Files);
                o.MigrationsAssembly(typeof(FileDbContext).Assembly.FullName);
            })
        );
        services.AddScoped<IFileDbContext>(provider => provider.GetRequiredService<FileDbContext>());
        return services;
    }
}

