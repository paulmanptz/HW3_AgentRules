using FluentResults;
using FluentValidation;
using MasterApp.Files.UseCases.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinks;

internal sealed class GetAttachmentLinksHandler(
    IFileDbContext fileDbContext,
    IFileDownloader fileDownloader)
    : IRequestHandler<GetAttachmentLinksQuery, Result<List<GetAttachmentLinksResponse>>>
{
    public async Task<Result<List<GetAttachmentLinksResponse>>> Handle(GetAttachmentLinksQuery query, CancellationToken cancellationToken)
    {
        var response = new List<GetAttachmentLinksResponse>();

        var files = await fileDbContext.Files.AsNoTracking()
            .Where(file => query.FileIds.Contains(file.Id))
            .ToListAsync(cancellationToken);

        if (files.Count == 0)
            return response;

        var names = files.Select(f => f.Id.ToString()).ToArray();
        var getAttachmentLinksResult = await fileDownloader.GetAttachmentLinksAsync(names, cancellationToken);
        if (getAttachmentLinksResult.IsFailed)
            return Result.Fail(getAttachmentLinksResult.Errors);
        var linkByName = getAttachmentLinksResult.Value;

        foreach (var link in linkByName)
        {
            var file = files.FirstOrDefault(x => x.Id == Guid.Parse(link.Key));
            response.Add(new GetAttachmentLinksResponse(
                Guid.Parse(link.Key),
                link.Value,
                file?.FileName ?? string.Empty,
                file?.CreatedAt ?? default));
        }
        return response;
    }
}



