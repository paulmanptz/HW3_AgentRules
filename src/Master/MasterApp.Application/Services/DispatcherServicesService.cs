using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Application.Interfaces;
using MasterApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Application.Services;

public class DispatcherServicesService : IDispatcherServicesService
{
    private readonly IAppDbContext _context;

    public DispatcherServicesService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Service>> GetServicesAsync(int orgId, CancellationToken cancellationToken)
    {
        return await _context.Services
            .Where(s => s.OrgId == orgId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Service> CreateServiceAsync(int orgId, CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            OrgId = orgId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync(cancellationToken);

        return service;
    }

    public async Task<Service> UpdateServiceAsync(int orgId, Guid id, UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId, cancellationToken);

        if (service == null)
            throw new Exception("Услуга не найдена.");

        service.Name = request.Name;
        service.Description = request.Description;
        service.Price = request.Price;

        await _context.SaveChangesAsync(cancellationToken);

        return service;
    }

    public async Task DeleteServiceAsync(int orgId, Guid id, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.OrgId == orgId, cancellationToken);

        if (service == null)
            throw new Exception("Услуга не найдена.");

        _context.Services.Remove(service);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
