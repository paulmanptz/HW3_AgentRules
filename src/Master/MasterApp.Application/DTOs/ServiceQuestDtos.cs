using System.Text.Json.Serialization;
using MasterApp.Domain.Enums;

namespace MasterApp.Application.DTOs;

public class QuestStatusCountDto
{
    public int StatusCode { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ServiceQuestStatus Status { get; set; }
    public int Count { get; set; }
}

public class ServiceQuestDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public ServiceQuestStatus Status { get; set; }
    public double Price { get; set; }
}

public class ServiceQuestDetailDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public ServiceQuestStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime Deadline { get; set; }
    
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientComment { get; set; } = string.Empty;
    public double FixedPrice { get; set; }

    public double? SpentTimeHours { get; set; }
    public string? CompletionComment { get; set; }
    public Guid? RelatedTicketId { get; set; }
    public List<QuestAttachmentDto> Attachments { get; set; } = new();
}

public class QuestAttachmentDto
{
    public Guid AttachmentId { get; set; }

    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
}

public class UpdateStatusRequest
{
    public ServiceQuestStatus NewStatus { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class CompleteReportRequest
{
    public double SpentTimeHours { get; set; }
    public string? CompletionComment { get; set; }
}

public class ScheduleQuestRequest
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class OccupiedIntervalDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? RequestNumber { get; set; }
}
