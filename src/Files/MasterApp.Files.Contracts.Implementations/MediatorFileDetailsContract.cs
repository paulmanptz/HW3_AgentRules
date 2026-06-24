using FluentResults;
using MasterApp.Files.UseCases.Queries.FileDetails.GetMaxFileCreatedAt;
using MediatR;

namespace MasterApp.Files.Contracts.Implementations;
internal sealed class MediatorFileDetailsContract(IMediator mediator) : IFileDetailsContract
{
    //Task<Result<DateTime>> IFileDetailsContract.GetMaxFileCreatedAt(IReadOnlyCollection<Guid> FileIds, CancellationToken cancellationToken)
    //    => mediator.Send(new GetMaxFileCreatedAtQuery(FileIds), cancellationToken)
    //    .Bind(response => FileDetailsMapper.ToResult(response).ToResult());

    async Task<Result<DateTime>> IFileDetailsContract.GetMaxFileCreatedAt(
    IReadOnlyCollection<Guid> FileIds,
    CancellationToken cancellationToken)
    {
        var query = new GetMaxFileCreatedAtQuery(FileIds);
        var res = await mediator.Send(query, cancellationToken).ConfigureAwait(false);

        return res.Value.CreatedAt;
    }
}



