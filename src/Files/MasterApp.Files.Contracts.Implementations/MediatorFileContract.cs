using FluentResults;
using FluentResults.Extensions;
using MasterApp.Files.Contracts.Models;
using MasterApp.Files.UseCases.Commands.DeleteFiles;
using MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinks;
using MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinksInDict;
using MediatR;

namespace MasterApp.Files.Contracts.Implementations;

internal sealed class MediatorFileContract(IMediator mediator) : IFileContract
{
    public Task<Result<Guid>> UploadFileAsync(UploadFileDto dto, CancellationToken cancellationToken = default)
        => mediator.Send(FileMapper.ToCommand(dto), cancellationToken);

    public Task<Result<List<FileDto>>> GetAttachmentLinksAsync(IReadOnlyCollection<Guid> FileIds, CancellationToken cancellationToken = default)
        => mediator.Send(new GetAttachmentLinksQuery(FileIds), cancellationToken)
        .Bind(response => FileMapper.ToDto(response).ToResult());

    public Task<Result<IReadOnlyDictionary<Guid, FileDto>>> GetAttachmentLinksInDictAsync(IReadOnlyCollection<Guid> FileIds, CancellationToken cancellationToken = default)
        => mediator.Send(new GetAttachmentLinksInDictQuery(FileIds), cancellationToken)
        .Bind(response => FileMapper.ToDtoDict(response).ToResult());
    public Task<Result> DeleteFilesAsync(IReadOnlyCollection<Guid> fileIds, CancellationToken cancellationToken = default)
        => mediator.Send(new DeleteFilesCommand(fileIds), cancellationToken);

}



