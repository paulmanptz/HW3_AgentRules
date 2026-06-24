using FluentResults;
using MasterApp.Files.Contracts.Models;

namespace MasterApp.Files.Contracts;

public interface IFileContract
{
    Task<Result<List<FileDto>>> GetAttachmentLinksAsync(IReadOnlyCollection<Guid> FileIds, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyDictionary<Guid, FileDto>>> GetAttachmentLinksInDictAsync(IReadOnlyCollection<Guid> FileIds, CancellationToken cancellationToken = default);
    Task<Result<Guid>> UploadFileAsync(UploadFileDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteFilesAsync(IReadOnlyCollection<Guid> fileIds, CancellationToken cancellationToken = default);
}


