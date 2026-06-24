using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MasterApp.Auth.Application.Interfaces;
using MasterApp.Auth.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MasterApp.Auth.Infrastructure.Services;

public class SsoTokenService : ISsoTokenService
{
    private readonly IConfiguration _configuration;

    public SsoTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateSsoToken(User user, string login)
    {
        var secretKey = _configuration["Sso:SharedSecretKey"]
            ?? throw new InvalidOperationException("Sso:SharedSecretKey is not configured");
        var issuer = _configuration["Sso:Issuer"] ?? "MasterApp";
        var audience = _configuration["Sso:Audience"] ?? "Domokey";
        var lifetimeMinutes = int.TryParse(_configuration["Sso:TokenLifetimeMinutes"], out var m) ? m : 5;

        var key = Encoding.ASCII.GetBytes(secretKey);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, login),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("login", login)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(lifetimeMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
