using MasterApp.Domain.Enums;

namespace MasterApp.Domain.Entities;

public class ServiceQuestStatusHistory
{
    public Guid Id { get; set; }
    public Guid StatusRequestId { get; set; }
    public DateTime ChangedAt { get; set; }
    public ServiceQuestStatus Status { get; set; }
    public ChangedByType ChangedBy { get; set; }
    public string Comment { get; set; } = string.Empty;

    public ServiceQuest? ServiceQuest { get; set; }
}
