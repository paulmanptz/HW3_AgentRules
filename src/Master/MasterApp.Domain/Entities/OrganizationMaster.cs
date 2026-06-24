using System;

namespace MasterApp.Domain.Entities;

public class OrganizationMaster
{
    public int OrgId { get; set; }
    public Guid UserId { get; set; }

    public Organization? Organization { get; set; }

}
