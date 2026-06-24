using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.DTOs;

namespace MasterApp.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid masterId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, Guid masterId, CancellationToken cancellationToken = default);
    Task CreateNotificationAsync(Guid masterId, string title, string message, Guid? relatedEntityId = null, string? relatedEntityType = null, CancellationToken cancellationToken = default);
}
