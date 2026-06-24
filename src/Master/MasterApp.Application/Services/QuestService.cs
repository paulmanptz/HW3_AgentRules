using MasterApp.Application.DTOs;
using MasterApp.Application.Interfaces;
using MasterApp.Auth.Application.Interfaces;
using MasterApp.Domain.Entities;
using MasterApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

using MasterApp.Files.Contracts;

namespace MasterApp.Application.Services;

public class QuestService : IQuestService
{
    private readonly IAppDbContext _context;
    private readonly IAuthDbContext _authContext;
    private readonly IFileContract _fileContract;

    public QuestService(IAppDbContext context, IAuthDbContext authContext, IFileContract fileContract)
    {
        _context = context;
        _authContext = authContext;
        _fileContract = fileContract;
    }

    public async Task<IEnumerable<ServiceQuestDto>> GetQuestsAsync(Guid masterId, ServiceQuestStatus? statusFilter, CancellationToken cancellationToken)
    {
        var query = _context.ServiceQuests.Where(r => r.MasterId == masterId);

        if (statusFilter.HasValue)
        {
            query = query.Where(r => r.Status == statusFilter.Value);
        }

        return await query.Select(r => new ServiceQuestDto
        {
            Id = r.Id,
            Number = r.Number,
            Title = r.Title,
            Address = r.Address,
            Deadline = r.Deadline,
            Status = r.Status,
            Price = r.FixedPrice
        }).ToListAsync(cancellationToken);
    }

    public async Task<ServiceQuestDetailDto> GetRequestDetailAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var request = await _context.ServiceQuests
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);
        if (request == null) throw new Exception("Request not found");

        var dto = new ServiceQuestDetailDto
        {
            Id = request.Id,
            Number = request.Number,
            Status = request.Status,
            Title = request.Title,
            Description = request.Description,
            CreatedAt = request.CreatedAt,
            Deadline = request.Deadline,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            ClientName = request.ClientName,
            ClientComment = request.ClientComment,
            FixedPrice = request.FixedPrice,
            ScheduledStartDate = request.ScheduledStartDate,
            ScheduledEndDate = request.ScheduledEndDate,
            SpentTimeHours = request.SpentTimeHours,
            CompletionComment = request.CompletionComment,
            RelatedTicketId = request.RelatedTicketId
        };

        if (request.Attachments.Count > 0)
            dto.Attachments = await MapAttachmentsAsync(request.Attachments, cancellationToken);

        return dto;
    }

    public async Task UpdateStatusAsync(Guid requestId, Guid masterId, UpdateStatusRequest updateRequest, CancellationToken cancellationToken)
    {
        var request = await _context.ServiceQuests.FirstOrDefaultAsync(r => r.Id == requestId && r.MasterId == masterId, cancellationToken);
        if (request == null) throw new Exception("Request not found");

        var now = DateTime.UtcNow;

        request.Status = updateRequest.NewStatus;
        if (IsFinishedStatus(request.Status))
            request.FinishedAt = now;

        var history = new ServiceQuestStatusHistory
        {
            Id = Guid.NewGuid(),
            StatusRequestId = request.Id,
            ChangedAt = now,
            Status = updateRequest.NewStatus,
            ChangedBy = ChangedByType.Master,
            Comment = updateRequest.Comment
        };

        _context.ServiceQuestStatusHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteReportAsync(Guid requestId, Guid masterId, CompleteReportRequest reportRequest, CancellationToken cancellationToken)
    {
        var request = await _context.ServiceQuests.FirstOrDefaultAsync(r => r.Id == requestId && r.MasterId == masterId, cancellationToken);
        if (request == null) throw new Exception("Request not found");

        request.SpentTimeHours = reportRequest.SpentTimeHours;
        request.CompletionComment = reportRequest.CompletionComment;

        if (request.Status != ServiceQuestStatus.Completed)
        {
            var now = DateTime.UtcNow;

            request.Status = ServiceQuestStatus.Completed;
            request.FinishedAt = now;
            var history = new ServiceQuestStatusHistory
            {
                Id = Guid.NewGuid(),
                StatusRequestId = request.Id,
                ChangedAt = now,
                Status = ServiceQuestStatus.Completed,
                ChangedBy = ChangedByType.Master,
                Comment = "Auto completed via report"
            };
            _context.ServiceQuestStatusHistories.Add(history);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ScheduleAsync(Guid requestId, Guid masterId, ScheduleQuestRequest request, CancellationToken cancellationToken)
    {
        var serviceQuest = await _context.ServiceQuests.FirstOrDefaultAsync(r => r.Id == requestId && r.MasterId == masterId, cancellationToken);
        if (serviceQuest == null) throw new Exception("Request not found");

        serviceQuest.ScheduledStartDate = request.StartTime;
        serviceQuest.ScheduledEndDate = request.EndTime;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnscheduleAsync(Guid requestId, Guid masterId, CancellationToken cancellationToken)
    {
        var serviceQuest = await _context.ServiceQuests.FirstOrDefaultAsync(r => r.Id == requestId && r.MasterId == masterId, cancellationToken);
        if (serviceQuest == null) throw new Exception("Request not found");

        serviceQuest.ScheduledStartDate = null;
        serviceQuest.ScheduledEndDate = null;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<OccupiedIntervalDto>> GetOccupiedScheduleAsync(Guid masterId, DateTime date, CancellationToken cancellationToken)
    {
        var startOfDay = date;
        var endOfDay = startOfDay.AddDays(1);

        var occupied = await _context.ServiceQuests
            .Where(r => r.MasterId == masterId 
                && (r.Status == ServiceQuestStatus.New || r.Status == ServiceQuestStatus.Assigned || r.Status == ServiceQuestStatus.InProgress)
                && r.ScheduledStartDate.HasValue 
                && r.ScheduledEndDate.HasValue
                && r.ScheduledStartDate.Value < endOfDay
                && r.ScheduledEndDate.Value > startOfDay)
            .Select(r => new OccupiedIntervalDto
            {
                StartTime = r.ScheduledStartDate!.Value,
                EndTime = r.ScheduledEndDate!.Value,
                RequestNumber = r.Number
            })
            .ToListAsync(cancellationToken);

        return occupied;
    }

    public async Task<QuestAttachmentDto> UploadAttachmentAsync(Guid questId, Guid masterId, Guid fileId, CancellationToken cancellationToken)
    {
        var quest = await _context.ServiceQuests
            .FirstOrDefaultAsync(q => q.Id == questId && q.MasterId == masterId, cancellationToken);

        if (quest == null)
            throw new Exception("Заявка не найдена или не принадлежит вам.");

        var attachment = new ServiceQuestAttachment
        {
            Id = Guid.NewGuid(),
            ServiceQuestId = questId,
            FileId = fileId,
            UserId = masterId,
            UploadedAt = DateTime.UtcNow
        };

        _context.ServiceQuestAttachments.Add(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        var mapped = await MapAttachmentsAsync([attachment], cancellationToken);
        return mapped[0];
    }

    private async Task<List<QuestAttachmentDto>> MapAttachmentsAsync(
        IReadOnlyList<ServiceQuestAttachment> attachments,
        CancellationToken cancellationToken)
    {
        if (attachments.Count == 0)
            return [];

        var fileIds = attachments.Select(a => a.FileId).ToList();
        var fileLinksResult = await _fileContract.GetAttachmentLinksInDictAsync(fileIds, cancellationToken);
        if (fileLinksResult.IsFailed)
            return [];

        var dict = fileLinksResult.Value;
        var userIds = attachments.Select(a => a.UserId).Distinct().ToList();
        var users = await _authContext.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Role })
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        return attachments.Select(a =>
        {
            dict.TryGetValue(a.FileId, out var fileDto);
            var user = users.GetValueOrDefault(a.UserId);
            var userName = string.Empty;
            if (user != null)
            {
                var parts = new[] { user.FirstName, user.LastName }.Where(p => !string.IsNullOrWhiteSpace(p));
                userName = string.Join(" ", parts);
            }

            return new QuestAttachmentDto
            {
                AttachmentId = a.Id,
                Url = fileDto?.Link ?? string.Empty,
                FileName = fileDto?.FileName ?? string.Empty,
                UploadedAt = a.UploadedAt,
                UserId = a.UserId,
                UserName = userName,
                UserRole = user?.Role.ToString() ?? string.Empty
            };
        }).ToList();
    }

    public async Task DeleteAttachmentAsync(Guid questId, Guid attachmentId, Guid masterId, CancellationToken cancellationToken)
    {
        var attachment = await _context.ServiceQuestAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.ServiceQuestId == questId, cancellationToken);

        if (attachment == null)
            throw new Exception("Вложение не найдено.");

        if (attachment.UserId != masterId)
            throw new Exception("Невозможно удалить файл, так как вы не являетесь автором вложения.");

        var quest = await _context.ServiceQuests
            .FirstOrDefaultAsync(q => q.Id == questId && q.MasterId == masterId, cancellationToken);

        if (quest == null)
            throw new Exception("Заявка не найдена или не принадлежит вам.");

        _context.ServiceQuestAttachments.Remove(attachment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static bool IsFinishedStatus(ServiceQuestStatus status) =>
        status is ServiceQuestStatus.Completed or ServiceQuestStatus.Cancelled;
}
