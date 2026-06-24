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
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<MasterProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var masterId = User.GetMasterId();
        var profile = await _profileService.GetProfileAsync(masterId, cancellationToken);
        return Ok(profile);
    }
}
