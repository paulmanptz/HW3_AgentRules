using MasterApp.Auth.Application.Interfaces;
using MasterApp.Auth.Domain.Entities;
using MasterApp.Auth.Infrastructure.Data;
using MasterApp.Files.Infrastructure;
using MasterApp.Files.UseCases;
using MasterApp.Infrastructure;
using MasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

Console.WriteLine("Building migrator...");

var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string is null or empty. Use '--ConnectionStrings:DefaultConnection' or ConnectionStrings__DefaultConnection env.");
}

var services = new ServiceCollection();
services.AddInfrastructure(configuration);
RegisterAuthDbContext(services, connectionString);
services.AddFilesInfrastructure(configuration);

using var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();

var dbContexts = new (string Name, DbContext Context)[]
{
    (nameof(AppDbContext), scope.ServiceProvider.GetRequiredService<AppDbContext>()),
    (nameof(AuthDbContext), scope.ServiceProvider.GetRequiredService<AuthDbContext>()),
    (nameof(IFileDbContext), GetFileDbContext(scope.ServiceProvider))
};

Console.WriteLine("Starting migration...");
foreach (var (name, dbContext) in dbContexts)
{
    var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
    if (pendingMigrations.Length == 0)
    {
        Console.WriteLine($"{name} no migrations to apply.");
        continue;
    }

    Console.WriteLine($"{name} pending migrations: {string.Join(", ", pendingMigrations)}");
    await dbContext.Database.MigrateAsync();
    Console.WriteLine($"{name} updated.");
}

Console.WriteLine("Migration done.");

static void RegisterAuthDbContext(IServiceCollection services, string connectionString)
{
    var authDataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    authDataSourceBuilder.MapEnum<RoleType>("Auth.RoleType");
    var authDataSource = authDataSourceBuilder.Build();

    services.AddDbContext<AuthDbContext>(options =>
        options.UseNpgsql(authDataSource, o => o.MapEnum<RoleType>("RoleType", "Auth")));
    services.AddScoped<IAuthDbContext>(provider => provider.GetRequiredService<AuthDbContext>());
}

static DbContext GetFileDbContext(IServiceProvider serviceProvider)
{
    return serviceProvider.GetRequiredService<IFileDbContext>() as DbContext
        ?? throw new InvalidOperationException($"{nameof(IFileDbContext)} implementation must inherit {nameof(DbContext)}.");
}
