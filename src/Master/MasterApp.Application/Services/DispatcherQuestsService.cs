using MasterApp.Application.DTOs;
using MasterApp.Application.Interfaces;
using MasterApp.Auth.Application.Interfaces;
using MasterApp.Domain.Entities;
using MasterApp.Domain.Enums;
using MasterApp.Files.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Application.Services;

public class DispatcherQuestsService : IDispatcherQuestsService
{
    private readonly IAppDbContext _context;
    private readonly IAuthDbContext _authContext;
    private readonly IFileContract _fileContract;

    public DispatcherQuestsService(IAppDbContext context, IAuthDbContext authContext, IFileContract fileContract)
    {
        _context = context;
        _authContext = authContext;
        _fileContract = fileContract;
    }

    public async Task<IReadOnlyList<QuestStatusCountDto>> GetQuestCountsByStatusAsync(int orgId, CancellationToken cancellationToken)
    {
        var countsByStatus = await _context.ServiceQuests
            .Where(r => r.OrgId == orgId && !r.IsDeleted)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var visibleStatuses = new[]
        {
            ServiceQuestStatus.New,
            ServiceQuestStatus.Assigned,
            ServiceQuestStatus.InProgress,
            ServiceQuestStatus.Completed,
            ServiceQuestStatus.Cancelled
        };

        return visibleStatuses
            .Select(status => new QuestStatusCountDto
            {
                StatusCode = (int)status,
                Status = status,
                Count = countsByStatus.FirstOrDefault(c => c.Status == status)?.Count ?? 0
            })
            .ToList();
    }

    public async Task<PaginatedResult<DispatcherQuestListItemDto>> GetQuestsAsync(int orgId, GetDispatcherQuestsFilter filter, CancellationToken cancellationToken)
    {
        var query = _context.ServiceQuests
            .Where(r => r.OrgId == orgId && !r.IsDeleted);

        if (filter.Statuses != null && filter.Statuses.Length > 0)
        {
            query = query.Where(r => filter.Statuses.Contains(r.Status));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new DispatcherQuestListItemDto
            {
                Id = r.Id,
                Number = r.Number,
                Status = r.Status,
                Title = r.Title,
                Description = r.Description,
                Address = r.Address,
                PhoneNumber = r.PhoneNumber,
                ClientName = r.ClientName,
                ClientComment = r.ClientComment,
                FixedPrice = r.FixedPrice,
                Deadline = r.Deadline,
                CreatedAt = r.CreatedAt,
                FinishedAt = r.FinishedAt,
                ScheduledStartDate = r.ScheduledStartDate,
                ScheduledEndDate = r.ScheduledEndDate,
                SpentTimeHours = r.SpentTimeHours,
                CompletionComment = r.CompletionComment,
                RelatedTicketId = r.RelatedTicketId,
                ServiceId = r.ServiceId,
                MasterId = r.MasterId
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<DispatcherQuestListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<DispatcherQuestDetailDto> GetQuestDetailAsync(int orgId, Guid id, CancellationToken cancellationToken)
    {
        var request = await _context.ServiceQuests
            .Include(r => r.StatusHistory)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId && !r.IsDeleted, cancellationToken);

        if (request == null)
            throw new Exception("Заявка не найдена.");

        var dto = new DispatcherQuestDetailDto
        {
            Id = request.Id,
            Number = request.Number,
            Status = request.Status,
            Title = request.Title,
            Description = request.Description,
            CreatedAt = request.CreatedAt,
            FinishedAt = request.FinishedAt,
            Deadline = request.Deadline,
            ScheduledStartDate = request.ScheduledStartDate,
            ScheduledEndDate = request.ScheduledEndDate,
            Address = request.Address,
            AccountNumber = request.AccountNumber,
            PhoneNumber = request.PhoneNumber,
            ClientName = request.ClientName,
            ClientComment = request.ClientComment,
            FixedPrice = request.FixedPrice,
            SpentTimeHours = request.SpentTimeHours,
            CompletionComment = request.CompletionComment,
            RelatedTicketId = request.RelatedTicketId,
            ServiceId = request.ServiceId,
            MasterId = request.MasterId,
            StatusHistory = request.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new StatusHistoryItemDto
                {
                    ChangedAt = h.ChangedAt,
                    Status = h.Status,
                    ChangedBy = h.ChangedBy,
                    Comment = h.Comment
                }).ToList()
        };

        if (request.Attachments.Count > 0)
        {
            var fileIds = request.Attachments.Select(a => a.FileId).ToList();
            var fileLinksResult = await _fileContract.GetAttachmentLinksInDictAsync(fileIds, cancellationToken);
            if (fileLinksResult.IsSuccess)
            {
                var dict = fileLinksResult.Value;
                // Получаем информацию о пользователях
                var userIds = request.Attachments.Select(a => a.UserId).Distinct().ToList();
                var users = await _authContext.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.FirstName, u.LastName, u.Role })
                    .ToDictionaryAsync(u => u.Id, cancellationToken);
                
                dto.Attachments = request.Attachments.Select(a =>
                {
                    dict.TryGetValue(a.FileId, out var fileDto);
                    var user = users.GetValueOrDefault(a.UserId);
                    string userName = string.Empty;
                    if (user != null)
                    {
                        var parts = new[] { user.FirstName, user.LastName }.Where(p => !string.IsNullOrWhiteSpace(p));
                        userName = string.Join(" ", parts);
                    }
                    return new AttachmentItem
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
        }

        return dto;
    }

    public async Task<DispatcherQuestListItemDto> CreateQuestAsync(int orgId, Guid userId, CreateDispatcherQuestDto dto, CancellationToken cancellationToken)
    {
        // Inside a transaction to safely generate the next Number
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var maxNumber = await _context.ServiceQuests
                .Where(r => r.OrgId == orgId)
                .OrderByDescending(r => r.Number)
                .Select(r => (int?)r.Number)
                .FirstOrDefaultAsync(cancellationToken) ?? 0;

            var newNumber = maxNumber + 1;

            var request = new ServiceQuest
            {
                Id = Guid.NewGuid(),
                OrgId = orgId,
                MasterId = dto.MasterId,
                ServiceId = dto.ServiceId,
                Number = newNumber,
                Status = ServiceQuestStatus.Assigned,
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                CreatedAt = DateTime.UtcNow,
                Address = dto.Address,
                AccountNumber = dto.AccountNumber,
                PhoneNumber = dto.PhoneNumber,
                ClientName = dto.ClientName,
                ClientComment = dto.ClientComment,
                FixedPrice = dto.FixedPrice,
                RelatedTicketId = dto.RelatedTicketId
            };

            // Add history
            var history = new ServiceQuestStatusHistory
            {
                Id = Guid.NewGuid(),
                StatusRequestId = request.Id,
                Status = ServiceQuestStatus.Assigned,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = ChangedByType.Organization, // Or Dispatcher if added to enum
                Comment = "Заявка создана диспетчером"
            };

            var notification = new MasterNotification
            {
                Id = Guid.NewGuid(),
                MasterId = request.MasterId,
                Title = "Новая заявка",
                Message = $"Вам назначена новая заявка № {request.Number}: {request.Title}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                RelatedEntityId = request.Id,
                RelatedEntityType = "ServiceQuest"
            };

            _context.ServiceQuests.Add(request);
            _context.ServiceQuestStatusHistories.Add(history);
            _context.MasterNotifications.Add(notification);

            if (request.RelatedTicketId.HasValue)
            {
                _context.ClientNotifications.Add(new ClientNotification
                {
                    Id = Guid.NewGuid(),
                    TicketId = request.RelatedTicketId.Value,
                    Title = "Создание обращения",
                    Message = $"Ваше обращение принято в работу по заявке № {request.Number}: {request.Title}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    RelatedEntityId = request.Id,
                    RelatedEntityType = "ServiceQuest"
                });
            }

            if (dto.AttachmentFileIds != null && dto.AttachmentFileIds.Any())
            {
                foreach (var fileId in dto.AttachmentFileIds)
                {
                    _context.ServiceQuestAttachments.Add(new ServiceQuestAttachment
                    {
                        Id = Guid.NewGuid(),
                        ServiceQuestId = request.Id,
                        FileId = fileId,
                        UserId = userId,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return MapToListItem(request);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<DispatcherQuestListItemDto> UpdateQuestAsync(int orgId, Guid userId, Guid id, UpdateDispatcherQuestDto dto, CancellationToken cancellationToken)
    {
        var request = await _context.ServiceQuests
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId, cancellationToken);

        if (request == null)
            throw new Exception("Заявка не найдена.");

        if (dto.Status == ServiceQuestStatus.Deleted)
            throw new Exception("Для удаления заявки используйте DELETE /api/dispatcher/quests/{id}.");

        var now = DateTime.UtcNow;

        request.MasterId = dto.MasterId;
        request.ServiceId = dto.ServiceId;
        request.Title = dto.Title;
        request.Description = dto.Description;
        request.Deadline = dto.Deadline;
        request.Address = dto.Address;
        request.AccountNumber = dto.AccountNumber;
        request.PhoneNumber = dto.PhoneNumber;
        request.ClientName = dto.ClientName;
        request.ClientComment = dto.ClientComment;
        request.FixedPrice = dto.FixedPrice;

        if (dto.Status.HasValue && dto.Status.Value != request.Status)
        {
            request.Status = dto.Status.Value;

            if (IsFinishedStatus(request.Status))
                request.FinishedAt = now;

            _context.ServiceQuestStatusHistories.Add(new ServiceQuestStatusHistory
            {
                Id = Guid.NewGuid(),
                StatusRequestId = request.Id,
                Status = request.Status,
                ChangedAt = now,
                ChangedBy = ChangedByType.Organization,
                Comment = "Статус изменён диспетчером"
            });
        }

        if (dto.AttachmentFileIds != null)
        {
            var existingFiles = request.Attachments.Select(a => a.FileId).ToList();
            var newFiles = dto.AttachmentFileIds.Except(existingFiles).ToList();
            var removedFiles = existingFiles.Except(dto.AttachmentFileIds).ToList();

            if (removedFiles.Any())
            {
                var toRemove = request.Attachments.Where(a => removedFiles.Contains(a.FileId)).ToList();
                _context.ServiceQuestAttachments.RemoveRange(toRemove);
            }

            if (newFiles.Any())
            {
                foreach (var fileId in newFiles)
                {
                    _context.ServiceQuestAttachments.Add(new ServiceQuestAttachment
                    {
                        Id = Guid.NewGuid(),
                        ServiceQuestId = request.Id,
                        FileId = fileId,
                        UserId = userId,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToListItem(request);
    }

    private static DispatcherQuestListItemDto MapToListItem(ServiceQuest request) => new()
    {
        Id = request.Id,
        Number = request.Number,
        Status = request.Status,
        Title = request.Title,
        Description = request.Description,
        Address = request.Address,
        PhoneNumber = request.PhoneNumber,
        ClientName = request.ClientName,
        ClientComment = request.ClientComment,
        FixedPrice = request.FixedPrice,
        Deadline = request.Deadline,
        CreatedAt = request.CreatedAt,
        FinishedAt = request.FinishedAt,
        ScheduledStartDate = request.ScheduledStartDate,
        ScheduledEndDate = request.ScheduledEndDate,
        SpentTimeHours = request.SpentTimeHours,
        CompletionComment = request.CompletionComment,
        RelatedTicketId = request.RelatedTicketId,
        ServiceId = request.ServiceId,
        MasterId = request.MasterId
    };

    public async Task CancelQuestAsync(int orgId, Guid id, CancellationToken cancellationToken)
    {
        var request = await _context.ServiceQuests
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId, cancellationToken);

        if (request == null)
            throw new Exception("Заявка не найдена.");

        if (request.Status == ServiceQuestStatus.Cancelled || request.Status == ServiceQuestStatus.Completed)
            throw new Exception("Заявка уже завершена или отменена.");

        var now = DateTime.UtcNow;

        request.Status = ServiceQuestStatus.Cancelled;
        request.FinishedAt = now;

        var history = new ServiceQuestStatusHistory
        {
            Id = Guid.NewGuid(),
            StatusRequestId = request.Id,
            Status = ServiceQuestStatus.Cancelled,
            ChangedAt = now,
            ChangedBy = ChangedByType.Organization,
            Comment = "Отменена диспетчером"
        };
        _context.ServiceQuestStatusHistories.Add(history);

        var notification = new MasterNotification
        {
            Id = Guid.NewGuid(),
            MasterId = request.MasterId,
            Title = "Отмена заявки",
            Message = $"Заявка № {request.Number} была отменена диспетчером.",
            IsRead = false,
            CreatedAt = now,
            RelatedEntityId = request.Id,
            RelatedEntityType = "ServiceQuest"
        };
        _context.MasterNotifications.Add(notification);

        if (request.RelatedTicketId.HasValue)
        {
            _context.ClientNotifications.Add(new ClientNotification
            {
                Id = Guid.NewGuid(),
                TicketId = request.RelatedTicketId.Value,
                Title = "Отмена обращения",
                Message = $"Обращение по заявке № {request.Number} было отменено.",
                IsRead = false,
                CreatedAt = now,
                RelatedEntityId = request.Id,
                RelatedEntityType = "ServiceQuest"
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteQuestAsync(int orgId, Guid id, CancellationToken cancellationToken)
    {
        var request = await _context.ServiceQuests
            .FirstOrDefaultAsync(r => r.Id == id && r.OrgId == orgId, cancellationToken);

        if (request == null)
            throw new Exception("Заявка не найдена.");

        if (request.IsDeleted)
            throw new Exception("Заявка уже удалена.");

        request.IsDeleted = true;
        request.DeletedAt = DateTime.UtcNow;
        request.Status = ServiceQuestStatus.Deleted;

        var history = new ServiceQuestStatusHistory
        {
            Id = Guid.NewGuid(),
            StatusRequestId = request.Id,
            Status = ServiceQuestStatus.Deleted,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = ChangedByType.Organization,
            Comment = "Заявка удалена диспетчером"
        };
        _context.ServiceQuestStatusHistories.Add(history);

        var notification = new MasterNotification
        {
            Id = Guid.NewGuid(),
            MasterId = request.MasterId,
            Title = "Удаление заявки",
            Message = $"Заявка № {request.Number} была удалена диспетчером.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = request.Id,
            RelatedEntityType = "ServiceQuest"
        };
        _context.MasterNotifications.Add(notification);

        if (request.RelatedTicketId.HasValue)
        {
            _context.ClientNotifications.Add(new ClientNotification
            {
                Id = Guid.NewGuid(),
                TicketId = request.RelatedTicketId.Value,
                Title = "Удаление обращения",
                Message = $"Обращение по заявке № {request.Number} было удалено.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                RelatedEntityId = request.Id,
                RelatedEntityType = "ServiceQuest"
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<QuestByTicketResponse> GetQuestsByTicketIdAsync(int orgId, Guid ticketId, CancellationToken cancellationToken)
    {
        var questRows = await _context.ServiceQuests
            .Where(q => q.OrgId == orgId && q.RelatedTicketId == ticketId && !q.IsDeleted)
            .Select(q => new
            {
                q.Id,
                q.Number,
                q.Title,
                q.Status
            })
            .ToListAsync(cancellationToken);

        var quests = questRows
            .Select(q => new QuestByTicketItem
            {
                QuestId = q.Id,
                Number = q.Number,
                Title = q.Title,
                QuestStatusName = GetQuestStatusName(q.Status)
            })
            .ToList();

        return new QuestByTicketResponse
        {
            Count = quests.Count,
            Quests = quests
        };
    }

    public async Task DeleteAttachmentAsync(int orgId, Guid questId, Guid attachmentId, Guid userId, CancellationToken cancellationToken)
    {
        var attachment = await _context.ServiceQuestAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.ServiceQuestId == questId, cancellationToken);

        if (attachment == null)
            throw new Exception("Вложение не найдено.");

        if (attachment.UserId != userId)
            throw new Exception("Невозможно удалить файл, так как вы не являетесь автором вложения.");

        var quest = await _context.ServiceQuests
            .FirstOrDefaultAsync(q => q.Id == questId && q.OrgId == orgId && !q.IsDeleted, cancellationToken);

        if (quest == null)
            throw new Exception("Заявка не найдена.");

        _context.ServiceQuestAttachments.Remove(attachment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> UploadAttachmentAsync(int orgId, Guid questId, Guid userId, Guid fileId, CancellationToken cancellationToken)
    {
        var quest = await _context.ServiceQuests
            .FirstOrDefaultAsync(q => q.Id == questId && q.OrgId == orgId && !q.IsDeleted, cancellationToken);

        if (quest == null)
            throw new Exception("Заявка не найдена.");

        var attachment = new ServiceQuestAttachment
        {
            Id = Guid.NewGuid(),
            ServiceQuestId = questId,
            FileId = fileId,
            UserId = userId,
            UploadedAt = DateTime.UtcNow
        };

        _context.ServiceQuestAttachments.Add(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        return attachment.Id;
    }

    private static string GetQuestStatusName(ServiceQuestStatus status) => status switch
    {
        ServiceQuestStatus.New => "Новая",
        ServiceQuestStatus.Assigned => "Назначена",
        ServiceQuestStatus.InProgress => "В работе",
        ServiceQuestStatus.Completed => "Выполнена",
        ServiceQuestStatus.Cancelled => "Отменена",
        ServiceQuestStatus.Deleted => "Удалена",
        _ => status.ToString()
    };

    private static bool IsFinishedStatus(ServiceQuestStatus status) =>
        status is ServiceQuestStatus.Completed or ServiceQuestStatus.Cancelled;
}
