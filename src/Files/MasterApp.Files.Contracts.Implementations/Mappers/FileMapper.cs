using MasterApp.Files.Contracts.Models;
using MasterApp.Files.UseCases.Commands.Files.UploadAttachment;
using MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinks;
using MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinksInDict;
using Riok.Mapperly.Abstractions;

namespace MasterApp.Files.Contracts.Implementations;

[Mapper]
internal static partial class FileMapper
{
    internal static partial UploadAttachmentCommand ToCommand(UploadFileDto dto);
    internal static partial List<FileDto> ToDto(List<GetAttachmentLinksResponse> response);
    internal static partial IReadOnlyDictionary<Guid, FileDto> ToDtoDict(IReadOnlyDictionary<Guid, GetAttachmentLinksInDictResponse> response);
}

