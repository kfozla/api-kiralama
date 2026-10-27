using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api_kiralama.Models;
using Microsoft.IdentityModel.Tokens;

namespace api_kiralama.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),
            new Claim(
                ClaimTypes.Name,
                user.Username
            ),
            new Claim(
                ClaimTypes.Role,
                user.Role
            )
        };

        var secret = _configuration["Jwt:Secret"]
                     ?? throw new InvalidOperationException(
                         "JWT secret yapılandırılmamış."
                     );

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64)
        );
    }

    public string HashRefreshToken(string token)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(token)
        );

        return Convert.ToHexString(hash);
    }
}