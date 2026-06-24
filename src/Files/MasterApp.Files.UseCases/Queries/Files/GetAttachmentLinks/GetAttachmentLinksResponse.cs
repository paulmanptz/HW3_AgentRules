namespace MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinks;
public sealed record GetAttachmentLinksResponse(
    Guid? Name,
    string Link,
    string FileName,
    DateTime CreatedAt
);

