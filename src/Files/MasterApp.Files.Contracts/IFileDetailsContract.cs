using FluentResults;

namespace MasterApp.Files.Contracts;
public interface IFileDetailsContract
{
    Task<Result<DateTime>> GetMaxFileCreatedAt(IReadOnlyCollection<Guid> FileIds, CancellationToken cancellationToken = default);
}


