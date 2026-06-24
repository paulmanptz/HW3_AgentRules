using MasterApp.Application.DTOs;
using MasterApp.Domain.Enums;

namespace MasterApp.Application.Interfaces;

public interface IQuestService
{
    Task<IEnumerable<ServiceQuestDto>> GetQuestsAsync(Guid masterId, ServiceQuestStatus? statusFilter, CancellationToken cancellationToken);
    Task<ServiceQuestDetailDto> GetRequestDetailAsync(Guid requestId, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid requestId, Guid masterId, UpdateStatusRequest request, CancellationToken cancellationToken);
    Task CompleteReportAsync(Guid requestId, Guid masterId, CompleteReportRequest request, CancellationToken cancellationToken);
    
    Task ScheduleAsync(Guid requestId, Guid masterId, ScheduleQuestRequest request, CancellationToken cancellationToken);
    Task UnscheduleAsync(Guid requestId, Guid masterId, CancellationToken cancellationToken);
    Task<List<OccupiedIntervalDto>> GetOccupiedScheduleAsync(Guid masterId, DateTime date, CancellationToken cancellationToken);
    Task DeleteAttachmentAsync(Guid questId, Guid attachmentId, Guid masterId, CancellationToken cancellationToken);
    Task<QuestAttachmentDto> UploadAttachmentAsync(Guid questId, Guid masterId, Guid fileId, CancellationToken cancellationToken);
}
