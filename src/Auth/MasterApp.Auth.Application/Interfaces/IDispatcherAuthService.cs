using System.Threading;
using System.Threading.Tasks;
using MasterApp.Auth.Application.DTOs;

namespace MasterApp.Auth.Application.Interfaces;

public interface IDispatcherAuthService
{
    Task<DispatcherAuthResponse> LoginAsync(DispatcherLoginRequest request, CancellationToken cancellationToken);
    Task<DispatcherAuthResponse> RefreshAsync(DispatcherRefreshRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(Guid dispatcherId, CancellationToken cancellationToken);
}
