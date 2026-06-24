using System;
using System.Collections.Generic;

namespace MasterApp.Domain.Entities;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? INN { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<OrganizationMaster> OrganizationMasters { get; set; } = new List<OrganizationMaster>();
}
