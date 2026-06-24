using FluentResults;
using MediatR;

namespace MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinksInDict;

public sealed record GetAttachmentLinksInDictQuery(IReadOnlyCollection<Guid> FileIds) : IRequest<Result<IReadOnlyDictionary<Guid, GetAttachmentLinksInDictResponse>>>;


