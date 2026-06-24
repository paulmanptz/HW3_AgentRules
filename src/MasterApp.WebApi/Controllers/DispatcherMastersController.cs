using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MasterApp.WebApi.Controllers;

[ApiController]
[Route("api/dispatcher/masters")]
[Authorize(Roles = "Dispatcher")]
public class DispatcherMastersController : ControllerBase
{
    private readonly IDispatcherMastersService _mastersService;

    public DispatcherMastersController(IDispatcherMastersService mastersService)
    {
        _mastersService = mastersService;
    }

    private int GetOrgId()
    {
        var orgIdClaim = User.Claims.FirstOrDefault(c => c.Type == "OrgId")?.Value;
        if (int.TryParse(orgIdClaim, out var orgId))
            return orgId;
        throw new Exception("Не удалось определить организацию диспетчера.");
    }

    [HttpPost]
    public async Task<IActionResult> CreateMaster([FromBody] CreateMasterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var master = await _mastersService.CreateMasterAsync(orgId, request, cancellationToken);
            return Ok(master);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMasters(CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var masters = await _mastersService.GetMastersAsync(orgId, cancellationToken);
            return Ok(masters);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{masterId}")]
    public async Task<IActionResult> UpdateMaster(Guid masterId, [FromBody] UpdateMasterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var master = await _mastersService.UpdateMasterAsync(orgId, masterId, request, cancellationToken);
            return Ok(master);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
