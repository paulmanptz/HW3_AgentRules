using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.DTOs;
using MasterApp.Application.Interfaces;
using MasterApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IAppDbContext _context;

    public NotificationService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid masterId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.MasterNotifications.Where(n => n.MasterId == masterId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                MasterId = n.MasterId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task MarkAsReadAsync(Guid id, Guid masterId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.MasterNotifications
            .FirstOrDefaultAsync(n => n.Id == id && n.MasterId == masterId, cancellationToken);
            
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CreateNotificationAsync(Guid masterId, string title, string message, Guid? relatedEntityId = null, string? relatedEntityType = null, CancellationToken cancellationToken = default)
    {
        var notification = new MasterNotification
        {
            Id = Guid.NewGuid(),
            MasterId = masterId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType
        };

        _context.MasterNotifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
