using System;

namespace MasterApp.Auth.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? DeviceId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpireAt { get; set; }
    public DateTime IssuedAt { get; set; }

    public User? User { get; set; }
    public Device? Device { get; set; }
}
