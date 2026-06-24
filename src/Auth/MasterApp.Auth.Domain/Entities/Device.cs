using System;

namespace MasterApp.Auth.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Token { get; set; }
    public string? DeviceId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public User? User { get; set; }
}
