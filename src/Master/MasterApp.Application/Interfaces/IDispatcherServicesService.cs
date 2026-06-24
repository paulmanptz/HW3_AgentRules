using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Domain.Entities;

namespace MasterApp.Application.Interfaces;

public class CreateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
}

public class UpdateServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
}

public interface IDispatcherServicesService
{
    Task<List<Service>> GetServicesAsync(int orgId, CancellationToken cancellationToken);
    Task<Service> CreateServiceAsync(int orgId, CreateServiceRequest request, CancellationToken cancellationToken);
    Task<Service> UpdateServiceAsync(int orgId, Guid id, UpdateServiceRequest request, CancellationToken cancellationToken);
    Task DeleteServiceAsync(int orgId, Guid id, CancellationToken cancellationToken);
}
