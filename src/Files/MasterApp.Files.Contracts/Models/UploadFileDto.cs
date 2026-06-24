namespace MasterApp.Files.Contracts.Models;

public sealed record UploadFileDto(Stream Stream, string ContentType, long Size, string FileName);
