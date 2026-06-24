using FluentResults;

namespace MasterApp.Files.UseCases.Services;

public interface IFileDownloader
{
    Task<Result<IReadOnlyDictionary<string, string>>> GetAttachmentLinksAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyDictionary<Guid, string>>> GetAttachmentLinksAsync(IReadOnlyCollection<Guid> fileIds, CancellationToken cancellationToken = default);
}


