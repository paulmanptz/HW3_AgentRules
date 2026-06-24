using FluentResults;
using MediatR;

namespace MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinks;

public sealed record GetAttachmentLinksQuery(IReadOnlyCollection<Guid> FileIds) : IRequest<Result<List<GetAttachmentLinksResponse>>>;


