using FluentResults;
using MediatR;

namespace MasterApp.Files.UseCases.Commands.Files.UploadAttachment;

public sealed record UploadAttachmentCommand(Stream Stream, string ContentType, long Size, string FileName) : IRequest<Result<Guid>>;


