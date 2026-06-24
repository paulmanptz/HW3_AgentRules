using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.DTOs;
using MasterApp.Auth.Application.Interfaces;
using MasterApp.Auth.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterApp.WebApi.Controllers;

[ApiController]
[Route("api/dispatcher/auth")]
public class DispatcherAuthController : ControllerBase
{
    private readonly IDispatcherAuthService _authService;

    public DispatcherAuthController(IDispatcherAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<DispatcherAuthResponse>> Login([FromBody] DispatcherLoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<DispatcherAuthResponse>> Refresh([FromBody] DispatcherRefreshRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RefreshAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize(Roles = "Dispatcher")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            var dispatcherIdStr = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                               ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            
            if (!System.Guid.TryParse(dispatcherIdStr, out var dispatcherId))
                return Unauthorized();
            
            await _authService.LogoutAsync(dispatcherId, cancellationToken);
            return NoContent();
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
