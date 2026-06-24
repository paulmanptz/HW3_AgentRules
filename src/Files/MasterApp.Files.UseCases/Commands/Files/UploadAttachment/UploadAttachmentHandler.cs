using FluentResults;
using MasterApp.Files.UseCases.Services;
using MediatR;

namespace MasterApp.Files.UseCases.Commands.Files.UploadAttachment;

internal sealed class UploadAttachmentHandler(
    IFileDbContext fileDbContext,
    IFileUploader fileUploader)
    : IRequestHandler<UploadAttachmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadAttachmentCommand command, CancellationToken cancellationToken)
    {
        var newFile = File.Create(command.FileName);
        await fileDbContext.Files.AddAsync(newFile, cancellationToken);

        var uploadResult = await fileUploader.UploadAttachmentAsync(
            stream: command.Stream,
            name: newFile.Id.ToString(),
            contentType: command.ContentType,
            size: command.Size,
            cancellationToken: cancellationToken);
        if (uploadResult.IsFailed)
            return uploadResult;

        await fileDbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(newFile.Id);
    }
}



