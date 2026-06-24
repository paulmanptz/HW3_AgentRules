using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MasterApp.Domain.Enums;

namespace MasterApp.Domain.Entities;

public class ServiceQuest
{
    public Guid Id { get; set; }
    public int OrgId { get; set; }
    public Guid MasterId { get; set; }
    public Guid ServiceId { get; set; }

    public int Number { get; set; }

    public ServiceQuestStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }

    public string Address { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;
    public string ClientComment { get; set; } = string.Empty;
    public double FixedPrice { get; set; }

    public double? SpentTimeHours { get; set; }
    public string? CompletionComment { get; set; }

    public Guid? RelatedTicketId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Service? Service { get; set; }
    public List<ServiceQuestStatusHistory> StatusHistory { get; set; } = new();

    [JsonIgnore]
    public List<ServiceQuestAttachment> Attachments { get; set; } = new();

    [NotMapped]
    [JsonPropertyName("attachments")]
    public List<AttachmentItem> AttachmentItems { get; set; } = new();
}
