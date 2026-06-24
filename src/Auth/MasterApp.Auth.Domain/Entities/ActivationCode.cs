using System;

namespace MasterApp.Auth.Domain.Entities;

public class ActivationCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime ExpireAt { get; set; }

    public User? User { get; set; }
}
