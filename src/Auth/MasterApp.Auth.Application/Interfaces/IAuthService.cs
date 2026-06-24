using System.Threading;
using System.Threading.Tasks;
using MasterApp.Auth.Application.DTOs;

namespace MasterApp.Auth.Application.Interfaces;

public interface IAuthService
{
    Task SendCodeAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
