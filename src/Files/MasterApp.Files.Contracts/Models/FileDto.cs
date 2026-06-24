namespace MasterApp.Files.Contracts.Models;
public sealed record FileDto(
    Guid Name,
    string Link,
    string FileName,
    DateTime CreatedAt
);

