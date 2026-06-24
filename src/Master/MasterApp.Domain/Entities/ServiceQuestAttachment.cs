using System;

namespace MasterApp.Domain.Entities;

public class ServiceQuestAttachment
{
    public Guid Id { get; set; }
    public Guid ServiceQuestId { get; set; }
    public ServiceQuest ServiceQuest { get; set; } = null!;
    public Guid FileId { get; set; }
    public Guid UserId { get; set; }
    public DateTime UploadedAt { get; set; }
}
