using System;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.DTOs;
using MasterApp.Auth.Application.Interfaces;
using MasterApp.Auth.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterApp.WebApi.Controllers;

[ApiController]
[Route("api/master/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromBody] string phoneNumber, CancellationToken cancellationToken)
    {
        await _authService.SendCodeAsync(phoneNumber, cancellationToken);
        return Ok(new { message = "Код отправлен" });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        //
        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
