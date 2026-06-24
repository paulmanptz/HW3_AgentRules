
namespace MasterApp.Files.Domain;

public class File
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; private set; }
    public string FileName { get; private set; } = string.Empty;

    public void SetCreated(DateTime createdAt)
    {
        CreatedAt = createdAt;
    }

    public static File Create(string fileName)
    {
        var file = new File(Guid.NewGuid(), fileName);
        file.SetCreated(DateTime.UtcNow);
        return file;
    }

    #region ctors

    private File(Guid id, string fileName)
    {
        Id = id;
        FileName = fileName;
    }

    private File()
    {
    }

    #endregion
}

