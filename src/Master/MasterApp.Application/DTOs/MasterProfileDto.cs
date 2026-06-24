using System;

namespace MasterApp.Application.DTOs;

public class MasterProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    
    // Stats
    public int NewRequestsCount { get; set; }
    public int InProgressRequestsCount { get; set; }
    public int CompletedRequestsCount { get; set; }
}
