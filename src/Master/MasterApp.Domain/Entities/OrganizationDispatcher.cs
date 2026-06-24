using System;

namespace MasterApp.Domain.Entities;

public class OrganizationDispatcher
{
    // The primary key is the dispatcher's ID, enforcing a 1:1 user-to-org constraint
    public Guid UserId { get; set; }
    
    public int OrgId { get; set; }
    public Organization Organization { get; set; } = null!;
}
