using FluentResults;

namespace MasterApp.Files.UseCases.Services;

public interface IFileRemover
{
    Task<Result> RemoveAttachmentAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken = default);
}


