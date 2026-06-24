using MasterApp.Auth.Domain.Entities;

namespace MasterApp.Auth.Application.Interfaces;

public interface ISsoTokenService
{
    string GenerateSsoToken(User user, string login);
}
