using FluentResults;
using MediatR;

namespace MasterApp.Files.Contracts.Commands;

public sealed record DeleteFilesCommand(IReadOnlyCollection<Guid> FileIds) : IRequest<Result>;


