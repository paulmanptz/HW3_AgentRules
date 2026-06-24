using MasterApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MasterApp.Auth.Application.Interfaces;

public interface IAuthDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<Device> Devices { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    DbSet<ActivationCode> ActivationCodes { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
