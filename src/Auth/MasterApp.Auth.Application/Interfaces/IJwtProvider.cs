using System.Collections.Generic;
using MasterApp.Auth.Domain.Entities;

namespace MasterApp.Auth.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user, IDictionary<string, string>? customClaims = null);
}
