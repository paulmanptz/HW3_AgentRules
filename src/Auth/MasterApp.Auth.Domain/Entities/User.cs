using System;
using System.Collections.Generic;

namespace MasterApp.Auth.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? Login { get; set; }
    public string? PasswordHash { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Patronymic { get; set; }
    public bool IsActive { get; set; } = true;
    public RoleType Role { get; set; }
    public int? DomokeyOrgId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<ActivationCode> ActivationCodes { get; set; } = new List<ActivationCode>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
}
