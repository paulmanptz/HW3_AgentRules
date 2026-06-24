using FluentResults;
using MasterApp.Files.UseCases.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Files.UseCases.Commands.DeleteFiles;

internal sealed class DeleteFilesHandler(
    IFileDbContext fileDbContext,
    IFileRemover fileRemover)
    : IRequestHandler<DeleteFilesCommand, Result>
{
    public async Task<Result> Handle(DeleteFilesCommand command, CancellationToken cancellationToken)
    {
        if (command.FileIds.Count == 0)
            return Result.Ok();

        var fileNames = command.FileIds.Select(id => id.ToString()).ToList();

        // 1. Remove from MinIO
        var removeFilesResult = await fileRemover.RemoveAttachmentAsync(fileNames, cancellationToken);
        if (removeFilesResult.IsFailed)
            return removeFilesResult;

        // 2. Remove from Database
        var filesToDelete = await fileDbContext.Files
            .Where(f => command.FileIds.Contains(f.Id))
            .ToListAsync(cancellationToken);

        fileDbContext.Files.RemoveRange(filesToDelete);
        await fileDbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}



