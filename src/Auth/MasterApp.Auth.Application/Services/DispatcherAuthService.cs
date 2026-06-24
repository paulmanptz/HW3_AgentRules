using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MasterApp.Auth.Application.DTOs;
using MasterApp.Auth.Application.Interfaces;

using MasterApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MasterApp.Auth.Application.Services;

public class DispatcherAuthService : IDispatcherAuthService
{
    private readonly IAuthDbContext _context;
    private readonly IJwtProvider _jwtProvider;
    private readonly ISsoTokenService _ssoTokenService;
    private readonly System.Collections.Generic.IEnumerable<IUserClaimsProvider> _claimsProviders;
    private readonly IConfiguration _configuration;

    public DispatcherAuthService(IAuthDbContext context, IJwtProvider jwtProvider, ISsoTokenService ssoTokenService, System.Collections.Generic.IEnumerable<IUserClaimsProvider> claimsProviders, IConfiguration configuration)
    {
        _context = context;
        _jwtProvider = jwtProvider;
        _ssoTokenService = ssoTokenService;
        _claimsProviders = claimsProviders;
        _configuration = configuration;
    }

    public async Task<DispatcherAuthResponse> LoginAsync(DispatcherLoginRequest request, CancellationToken cancellationToken)
    {
        var targetRole = RoleType.Dispatcher;
        var dispatcher = await _context.Users
            .FirstOrDefaultAsync(d => d.Login == request.Login && d.Role == targetRole, cancellationToken);

        if (dispatcher == null)
            throw new Exception("Неверный логин или пароль");

        // Compute SHA256 of the provided password to compare with PasswordHash in DB
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
        var hashStr = Convert.ToHexString(hashedBytes).ToLower();

        if (dispatcher.PasswordHash?.ToLower() != hashStr)
            throw new Exception("Неверный логин или пароль");

        var customClaims = new System.Collections.Generic.Dictionary<string, string>();
        foreach (var provider in _claimsProviders)
        {
            var pClaims = await provider.GetAdditionalClaimsAsync(dispatcher, cancellationToken);
            foreach (var kvp in pClaims) customClaims[kvp.Key] = kvp.Value;
        }

        var token = _jwtProvider.GenerateToken(dispatcher, customClaims);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var rtEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            ExpireAt = DateTime.UtcNow.AddDays(7),
            UserId = dispatcher.Id,
            IssuedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(rtEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new DispatcherAuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = dispatcher.Id,
            SsoToken = _ssoTokenService.GenerateSsoToken(dispatcher, request.Login),
            DomokeyApiUrl = _configuration["Domokey:ApiUrl"],
            DomokeyOrgId = dispatcher.DomokeyOrgId
        };
    }

    public async Task<DispatcherAuthResponse> RefreshAsync(DispatcherRefreshRequest request, CancellationToken cancellationToken)
    {
        var rtEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (rtEntity == null || rtEntity.ExpireAt <= DateTime.UtcNow)
        {
            throw new Exception("Недействительный или просроченный Refresh-токен.");
        }

        var dispatcher = rtEntity.User ?? throw new Exception("User not found for token.");
        
        var customClaims = new System.Collections.Generic.Dictionary<string, string>();
        foreach (var provider in _claimsProviders)
        {
            var pClaims = await provider.GetAdditionalClaimsAsync(dispatcher, cancellationToken);
            foreach (var kvp in pClaims) customClaims[kvp.Key] = kvp.Value;
        }

        var newToken = _jwtProvider.GenerateToken(dispatcher, customClaims);
        var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        rtEntity.Token = newRefreshToken;
        rtEntity.ExpireAt = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        return new DispatcherAuthResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            UserId = dispatcher.Id,
            SsoToken = _ssoTokenService.GenerateSsoToken(dispatcher, dispatcher.Login ?? string.Empty),
            DomokeyApiUrl = _configuration["Domokey:ApiUrl"],
            DomokeyOrgId = dispatcher.DomokeyOrgId
        };
    }

    public async Task LogoutAsync(Guid dispatcherId, CancellationToken cancellationToken)
    {
        var rtList = await _context.RefreshTokens
            .Where(rt => rt.UserId == dispatcherId)
            .ToListAsync(cancellationToken);
            
        if (rtList.Any())
        {
            _context.RefreshTokens.RemoveRange(rtList);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
