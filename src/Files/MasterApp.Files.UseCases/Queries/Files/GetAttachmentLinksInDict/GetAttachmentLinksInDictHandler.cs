using FluentResults;
using FluentValidation;
using MasterApp.Files.UseCases.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinksInDict;
internal sealed class GetAttachmentLinksInDictHandler(
    IFileDbContext fileDbContext,
    IFileDownloader fileDownloader)
    : IRequestHandler<GetAttachmentLinksInDictQuery, Result<IReadOnlyDictionary<Guid, GetAttachmentLinksInDictResponse>>>
{

    public async Task<Result<IReadOnlyDictionary<Guid, GetAttachmentLinksInDictResponse>>> Handle(GetAttachmentLinksInDictQuery query, CancellationToken cancellationToken)
    {
        var response = new Dictionary<Guid, GetAttachmentLinksInDictResponse>();

        var files = await fileDbContext.Files
            .AsNoTracking()
            .Where(file => query.FileIds.Contains(file.Id))
            .ToDictionaryAsync(
                keySelector: file => file.Id,
                elementSelector: file => file,
                cancellationToken: cancellationToken);

        if (files.Count == 0)
            return response;

        var getAttachmentLinksResult = await fileDownloader.GetAttachmentLinksAsync(files.Keys, cancellationToken);
        if (getAttachmentLinksResult.IsFailed)
            return Result.Fail(getAttachmentLinksResult.Errors);
        var linkByName = getAttachmentLinksResult.Value;

        foreach (var link in linkByName)
        {
            var file = files.Values.FirstOrDefault(x => x.Id == link.Key);
            response.Add(link.Key, new GetAttachmentLinksInDictResponse(
                link.Key,
                link.Value,
                file?.FileName ?? string.Empty,
                file?.CreatedAt ?? default));
        }
        return response;
    }

}



