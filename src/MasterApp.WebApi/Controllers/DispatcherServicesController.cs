using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MasterApp.WebApi.Controllers;

[ApiController]
[Route("api/dispatcher/services")]
[Authorize(Roles = "Dispatcher")]
public class DispatcherServicesController : ControllerBase
{
    private readonly IDispatcherServicesService _servicesService;

    public DispatcherServicesController(IDispatcherServicesService servicesService)
    {
        _servicesService = servicesService;
    }

    private int GetOrgId()
    {
        var orgIdClaim = User.Claims.FirstOrDefault(c => c.Type == "OrgId")?.Value;
        if (int.TryParse(orgIdClaim, out var orgId))
            return orgId;
        throw new Exception("Не удалось определить организацию диспетчера.");
    }

    [HttpGet]
    public async Task<IActionResult> GetServices(CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var services = await _servicesService.GetServicesAsync(orgId, cancellationToken);
            return Ok(services);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var service = await _servicesService.CreateServiceAsync(orgId, request, cancellationToken);
            return Ok(service);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            var service = await _servicesService.UpdateServiceAsync(orgId, id, request, cancellationToken);
            return Ok(service);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var orgId = GetOrgId();
            await _servicesService.DeleteServiceAsync(orgId, id, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
