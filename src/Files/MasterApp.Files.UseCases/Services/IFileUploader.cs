using FluentResults;

namespace MasterApp.Files.UseCases.Services;

public interface IFileUploader
{
    Task<Result> UploadAttachmentAsync(Stream stream, string name, string contentType, long size, CancellationToken cancellationToken = default);
}


