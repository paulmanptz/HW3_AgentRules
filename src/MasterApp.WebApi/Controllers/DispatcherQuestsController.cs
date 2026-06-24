using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.DTOs;
using MasterApp.Application.Interfaces;
using MasterApp.Domain.Enums;
using MasterApp.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using MediatR;
using MasterApp.Files.UseCases.Commands.Files.UploadAttachment;
using FluentResults;

namespace MasterApp.WebApi.Controllers;

[ApiController]
[Route("api/dispatcher/quests")]
[Authorize(Roles = "Dispatcher")]
public class DispatcherQuestsController : ControllerBase
{
    private readonly IDispatcherQuestsService _requestsService;
    private readonly IMediator _mediator;

    public DispatcherQuestsController(IDispatcherQuestsService requestsService, IMediator mediator)
    {
        _requestsService = requestsService;
        _mediator = mediator;
    }

    private int GetOrgId()
    {
        var orgIdClaim = User.Claims.FirstOrDefault(c => c.Type == "OrgId")?.Value;
        if (int.TryParse(orgIdClaim, out var orgId))
            return orgId;
        throw new Exception("Не удалось определить организацию диспетчера.");
    }

    [HttpGet("counts-by-status")]
    public async Task<IActionResult> GetCountsByStatus(CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var counts = await _requestsService.GetQuestCountsByStatusAsync(orgId, cancellationToken);
            return Ok(counts);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequests([FromQuery] GetDispatcherQuestsFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var result = await _requestsService.GetQuestsAsync(orgId, filter, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequestDetail(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var result = await _requestsService.GetQuestDetailAsync(orgId, id, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateDispatcherQuestDto requestDto, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var userId = User.GetMasterId();
            var request = await _requestsService.CreateQuestAsync(orgId, userId, requestDto, cancellationToken);
            return Ok(request);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRequest(Guid id, [FromBody] UpdateDispatcherQuestDto requestDto, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var userId = User.GetMasterId();
            var request = await _requestsService.UpdateQuestAsync(orgId, userId, id, requestDto, cancellationToken);
            return Ok(request);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelRequest(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            await _requestsService.CancelQuestAsync(orgId, id, cancellationToken);
            return Ok(new { message = "Заявка успешно отменена" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRequest(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            await _requestsService.DeleteQuestAsync(orgId, id, cancellationToken);
            return Ok(new { message = "Заявка успешно удалена" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{questId}/attachments/{attachmentId}")]
    public async Task<IActionResult> DeleteAttachment(Guid questId, Guid attachmentId, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var userId = User.GetMasterId();
            await _requestsService.DeleteAttachmentAsync(orgId, questId, attachmentId, userId, cancellationToken);
            return Ok(new { message = "Вложение успешно удалено" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("by-ticket/{ticketId}")]
    public async Task<IActionResult> GetQuestsByTicketId(Guid ticketId, CancellationToken cancellationToken)
    {
        var orgId = GetOrgId();
        var result = await _requestsService.GetQuestsByTicketIdAsync(orgId, ticketId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{questId}/upload-attachment")]
    public async Task<IActionResult> UploadAttachmentToQuest(Guid questId, IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var command = new UploadAttachmentCommand(stream, file.ContentType, file.Length, file.FileName);
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsFailed)
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });

            var orgId = GetOrgId();
            var userId = User.GetMasterId();
            var attachmentId = await _requestsService.UploadAttachmentAsync(orgId, questId, userId, result.Value, cancellationToken);
                
            return Ok(new { fileId = result.Value, attachmentId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("upload-attachment-newquest")]
    public async Task<IActionResult> UploadAttachmentAsync(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var command = new UploadAttachmentCommand(stream, file.ContentType, file.Length, file.FileName);
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsFailed)
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });
                
            return Ok(new { fileId = result.Value });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
