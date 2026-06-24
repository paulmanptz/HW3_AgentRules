using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Auth.Domain.Entities;

namespace MasterApp.Auth.Application.Interfaces;

public interface IUserClaimsProvider
{
    Task<IDictionary<string, string>> GetAdditionalClaimsAsync(User user, CancellationToken cancellationToken);
}
