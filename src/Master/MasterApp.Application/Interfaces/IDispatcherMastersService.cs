using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Domain.Entities;
using MasterApp.Auth.Domain.Entities;

namespace MasterApp.Application.Interfaces;

public class CreateMasterRequest
{
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class UpdateMasterRequest
{
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class MasterDto
{
    public Guid Id { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Patronymic { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public interface IDispatcherMastersService
{
    Task<User> CreateMasterAsync(int orgId, CreateMasterRequest request, CancellationToken cancellationToken);
    Task<List<MasterDto>> GetMastersAsync(int orgId, CancellationToken cancellationToken);
    Task<User> UpdateMasterAsync(int orgId, Guid masterId, UpdateMasterRequest request, CancellationToken cancellationToken);
}
