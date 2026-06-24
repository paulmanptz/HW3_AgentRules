namespace MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinksInDict;

public sealed record GetAttachmentLinksInDictResponse(
    Guid? Name,
    string Link,
    string FileName,
    DateTime CreatedAt
);

