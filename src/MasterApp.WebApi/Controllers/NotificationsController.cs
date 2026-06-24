using System;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.DTOs;
using MasterApp.Application.Interfaces;
using MasterApp.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterApp.WebApi.Controllers;

[ApiController]
[Route("api/master/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var masterId = User.GetMasterId();
        var result = await _notificationService.GetNotificationsAsync(masterId, page, limit, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var masterId = User.GetMasterId();
        await _notificationService.MarkAsReadAsync(id, masterId, cancellationToken);
        return NoContent();
    }
}
