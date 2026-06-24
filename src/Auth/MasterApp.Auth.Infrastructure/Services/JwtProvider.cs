using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MasterApp.Auth.Application.Interfaces;

using MasterApp.Auth.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MasterApp.Auth.Infrastructure.Services;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user, IDictionary<string, string>? customClaims = null)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? "super_secret_key_which_is_very_long_for_hmac_sha256_at_least_32_bytes";
        var issuer = _configuration["Jwt:Issuer"] ?? "MasterApp";
        var audience = _configuration["Jwt:Audience"] ?? "MasterAppMobile";

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        if (customClaims != null)
        {
            foreach (var kvp in customClaims)
            {
                claims.Add(new Claim(kvp.Key, kvp.Value));
            }
        }
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
