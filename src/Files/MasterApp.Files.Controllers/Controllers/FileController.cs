using FluentResults;
using FluentResults.Extensions;
using MasterApp.Files.UseCases.Commands.Files.UploadAttachment;
using MasterApp.Files.UseCases.Queries.Files.GetAttachmentLinks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MasterApp.Files.Controllers.Controllers;

[ApiController]
[Route("/file-test")]
public sealed class FileController(IMediator mediator) : ControllerBase
{
    [HttpPost("/upload-meter-reading-photo")]
    public async Task<IActionResult> UploadAttachmentAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var stream = file.OpenReadStream();
        var command = new UploadAttachmentCommand(stream, file.ContentType, file.Length, file.FileName);
        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailed) return BadRequest(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost("/upload-task-attachment")]
    public async Task<IActionResult> UploadTaskAttachmentAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var stream = file.OpenReadStream();
        // reusing the existing command since they go to the same bucket
        var command = new UploadAttachmentCommand(stream, file.ContentType, file.Length, file.FileName);
        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailed) return BadRequest(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost("/get-meter-reading-photo-links")]
    public async Task<IActionResult> GetMeterReadingPhotoLinksAsync([FromForm] GetAttachmentLinksQuery query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        if (result.IsFailed) return BadRequest(result.Errors);
        return Ok(result.Value);
    }
}



