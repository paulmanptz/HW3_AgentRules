using MasterApp.Files.Infrastructure.Database;
using MasterApp.Files.UseCases;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Files.Infrastructure;
internal sealed class FileDbContext(DbContextOptions<FileDbContext> options) : DbContext(options), IFileDbContext
{
    public DbSet<File> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Files);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

