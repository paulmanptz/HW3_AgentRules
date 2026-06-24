using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.DTOs;
using MasterApp.Application.Interfaces;
using MasterApp.Domain.Enums;
using MasterApp.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MediatR;
using MasterApp.Files.UseCases.Commands.Files.UploadAttachment;
using FluentResults;

namespace MasterApp.WebApi.Controllers;

[ApiController]
[Route("api/master/quests")]
[Authorize]
public class QuestsController : ControllerBase
{
    private readonly IQuestService _requestService;
    private readonly IMediator _mediator;

    public QuestsController(IQuestService requestService, IMediator mediator)
    {
        _requestService = requestService;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceQuestDto>>> GetRequests([FromQuery] ServiceQuestStatus? status, CancellationToken cancellationToken)
    {
        var masterId = User.GetMasterId();
        var requests = await _requestService.GetQuestsAsync(masterId, status, cancellationToken);
        return Ok(requests);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceQuestDetailDto>> GetRequestDetail(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _requestService.GetRequestDetailAsync(id, cancellationToken);
        return Ok(detail);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var masterId = User.GetMasterId();
        await _requestService.UpdateStatusAsync(id, masterId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/report")]
    public async Task<IActionResult> CompleteReport(Guid id, [FromBody] CompleteReportRequest request, CancellationToken cancellationToken)
    {
        var masterId = User.GetMasterId();
        await _requestService.CompleteReportAsync(id, masterId, request, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/act")]
    public IActionResult GenerateAct(Guid id)
    {
        // Stub for act generation
        return Ok(new { message = "Акт выполненных работ успешно сформирован", documentUrl = $"/docs/act_{id}.pdf" });
    }

    [HttpPost("{id}/schedule")]
    public async Task<IActionResult> ScheduleQuest(Guid id, [FromBody] ScheduleQuestRequest request, CancellationToken cancellationToken)
    {
        var masterId = User.GetMasterId();
        await _requestService.ScheduleAsync(id, masterId, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/schedule")]
    public async Task<IActionResult> UnscheduleQuest(Guid id, CancellationToken cancellationToken)
    {
        var masterId = User.GetMasterId();
        await _requestService.UnscheduleAsync(id, masterId, cancellationToken);
        return NoContent();
    }

    [HttpGet("schedule")]
    public async Task<ActionResult<IEnumerable<OccupiedIntervalDto>>> GetSchedule([FromQuery] DateTime date, CancellationToken cancellationToken)
    {
        var masterId = User.GetMasterId();
        var schedule = await _requestService.GetOccupiedScheduleAsync(masterId, date, cancellationToken);
        return Ok(schedule);
    }

    [HttpGet("debug/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> DebugGet(Guid id, CancellationToken cancellationToken)
    {
        var request = await _requestService.GetRequestDetailAsync(id, cancellationToken);
        return Ok(request);
    }

    [HttpPost("{questId}/upload-attachment")]
    public async Task<IActionResult> UploadAttachmentAsync(Guid questId, IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var command = new UploadAttachmentCommand(stream, file.ContentType, file.Length, file.FileName);
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsFailed)
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });

            var masterId = User.GetMasterId();
            var attachment = await _requestService.UploadAttachmentAsync(questId, masterId, result.Value, cancellationToken);

            return Ok(attachment);
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
            var masterId = User.GetMasterId();
            await _requestService.DeleteAttachmentAsync(questId, attachmentId, masterId, cancellationToken);
            return Ok(new { message = "Вложение успешно удалено" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
