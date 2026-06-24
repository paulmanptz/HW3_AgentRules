using FluentResults;
using MasterApp.Files.Contracts.Commands;
using MasterApp.Files.UseCases;
using MasterApp.Files.UseCases.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Files.UseCases.Commands.Files.DeleteFiles;

internal sealed class DeleteFilesHandler(
    IFileDbContext fileDbContext,
    IFileRemover fileRemover)
    : IRequestHandler<DeleteFilesCommand, Result>
{
    public async Task<Result> Handle(DeleteFilesCommand command, CancellationToken cancellationToken)
    {
        if (command.FileIds.Count == 0)
            return Result.Ok();

        // 1. MinIO cleanup
        var fileNames = command.FileIds.Select(id => id.ToString()).ToList();
        var removeFilesResult = await fileRemover.RemoveAttachmentAsync(fileNames, cancellationToken);
        if (removeFilesResult.IsFailed)
            return removeFilesResult;

        // 2. File table cleanup
        var filesToDelete = await fileDbContext.Files
            .Where(f => command.FileIds.Contains(f.Id))
            .ToListAsync(cancellationToken);
        
        fileDbContext.Files.RemoveRange(filesToDelete);
        await fileDbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}



