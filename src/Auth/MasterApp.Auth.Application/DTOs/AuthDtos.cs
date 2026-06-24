using System;

namespace MasterApp.Auth.Application.DTOs;

public class LoginRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class DispatcherLoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class DispatcherAuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? SsoToken { get; set; }
    public string? DomokeyApiUrl { get; set; }
    public int? DomokeyOrgId { get; set; }
}

public class DispatcherRefreshRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
