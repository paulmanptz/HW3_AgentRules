using System;

namespace MasterApp.Domain.Entities;

public class Service
{
    public Guid Id { get; set; }
    public int OrgId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
}
