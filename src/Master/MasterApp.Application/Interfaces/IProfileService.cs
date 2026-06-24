using System;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.DTOs;

namespace MasterApp.Application.Interfaces;

public interface IProfileService
{
    Task<MasterProfileDto> GetProfileAsync(Guid masterId, CancellationToken cancellationToken);
}
