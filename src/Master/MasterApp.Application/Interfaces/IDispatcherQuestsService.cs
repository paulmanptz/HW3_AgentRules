using System.Text.Json.Serialization;
using MasterApp.Application.DTOs;
using MasterApp.Domain.Entities;
using MasterApp.Domain.Enums;

namespace MasterApp.Application.Interfaces;

public class GetDispatcherQuestsFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public ServiceQuestStatus[]? Statuses { get; set; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class CreateDispatcherQuestDto
{
    public Guid MasterId { get; set; }
    public Guid ServiceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public string Address { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientComment { get; set; } = string.Empty;
    public double FixedPrice { get; set; }
    public List<Guid>? AttachmentFileIds { get; set; }
    public Guid? RelatedTicketId { get; set; }
}

public class UpdateDispatcherQuestDto
{
    public Guid MasterId { get; set; }
    public Guid ServiceId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ServiceQuestStatus? Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public string Address { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientComment { get; set; } = string.Empty;
    public double FixedPrice { get; set; }
    public List<Guid>? AttachmentFileIds { get; set; }
}

public class DispatcherQuestListItemDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ServiceQuestStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientComment { get; set; } = string.Empty;
    public double FixedPrice { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public double? SpentTimeHours { get; set; }
    public string? CompletionComment { get; set; }
    public Guid? RelatedTicketId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid MasterId { get; set; }
}

public class DispatcherQuestDetailDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ServiceQuestStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime Deadline { get; set; }
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
    public Guid ServiceId { get; set; }
    public Guid MasterId { get; set; }
    public List<StatusHistoryItemDto> StatusHistory { get; set; } = new();
    public List<AttachmentItem> Attachments { get; set; } = new();
}

public class StatusHistoryItemDto
{
    public DateTime ChangedAt { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ServiceQuestStatus Status { get; set; }
    public ChangedByType ChangedBy { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class QuestByTicketItem
{
    public Guid QuestId { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string QuestStatusName { get; set; } = string.Empty;
}

public class QuestByTicketResponse
{
    public int Count { get; set; }
    public List<QuestByTicketItem> Quests { get; set; } = new();
}

public interface IDispatcherQuestsService
{
    Task<IReadOnlyList<QuestStatusCountDto>> GetQuestCountsByStatusAsync(int orgId, CancellationToken cancellationToken);
    Task<PaginatedResult<DispatcherQuestListItemDto>> GetQuestsAsync(int orgId, GetDispatcherQuestsFilter filter, CancellationToken cancellationToken);
    Task<DispatcherQuestDetailDto> GetQuestDetailAsync(int orgId, Guid id, CancellationToken cancellationToken);
    Task<DispatcherQuestListItemDto> CreateQuestAsync(int orgId, Guid userId, CreateDispatcherQuestDto requestDto, CancellationToken cancellationToken);
    Task<DispatcherQuestListItemDto> UpdateQuestAsync(int orgId, Guid userId, Guid id, UpdateDispatcherQuestDto requestDto, CancellationToken cancellationToken);
    Task CancelQuestAsync(int orgId, Guid id, CancellationToken cancellationToken);
    Task DeleteQuestAsync(int orgId, Guid id, CancellationToken cancellationToken);
    Task DeleteAttachmentAsync(int orgId, Guid questId, Guid attachmentId, Guid userId, CancellationToken cancellationToken);
    Task<QuestByTicketResponse> GetQuestsByTicketIdAsync(int orgId, Guid ticketId, CancellationToken cancellationToken);
    Task<Guid> UploadAttachmentAsync(int orgId, Guid questId, Guid userId, Guid fileId, CancellationToken cancellationToken);
}
