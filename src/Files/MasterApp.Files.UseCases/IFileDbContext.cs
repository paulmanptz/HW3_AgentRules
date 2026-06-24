using Microsoft.EntityFrameworkCore;

namespace MasterApp.Files.UseCases;

public interface IFileDbContext
{
    DbSet<File> Files { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

