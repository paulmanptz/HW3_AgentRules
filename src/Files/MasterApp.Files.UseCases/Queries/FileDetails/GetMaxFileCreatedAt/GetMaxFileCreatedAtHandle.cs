using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Files.UseCases.Queries.FileDetails.GetMaxFileCreatedAt;
internal sealed class GetMaxFileCreatedAtHandler(
    IFileDbContext fileDbContext)
    : IRequestHandler<GetMaxFileCreatedAtQuery, Result<GetMaxFileCreatedAtResponse>>
{
    public async Task<Result<GetMaxFileCreatedAtResponse>> Handle(GetMaxFileCreatedAtQuery query, CancellationToken cancellationToken)
    {
        var maxCreatedAt = await fileDbContext.Files
            .Where(f => query.FileIds.Contains(f.Id))
            .MaxAsync(f => f.CreatedAt, cancellationToken);

        var response = new GetMaxFileCreatedAtResponse(maxCreatedAt);

        return response;
    }
}




