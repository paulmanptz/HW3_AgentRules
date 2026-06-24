using FluentResults;
using MediatR;

namespace MasterApp.Files.UseCases.Commands.DeleteFiles;

public sealed record DeleteFilesCommand(IReadOnlyCollection<Guid> FileIds) : IRequest<Result>;


