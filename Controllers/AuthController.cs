
using System.Security.Claims;
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginRequest = api_kiralama.Dtos.LoginRequest;
using Microsoft.AspNetCore.RateLimiting;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    private const string RefreshCookieName = "rt";
    private const string RefreshCookiePath = "/api/auth";
    private const int RefreshTokenDays = 30;

    public AuthController(
        AppDbContext context,
        JwtService jwtService,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _environment = environment;
        _configuration = configuration;
    }

    private CookieOptions GetCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = RefreshCookiePath,
            Expires = DateTimeOffset.UtcNow.AddDays(
                RefreshTokenDays
            ),
            IsEssential = true
        };
    }

    private void SetRefreshCookie(string token)
    {
        Response.Cookies.Append(
            RefreshCookieName,
            token,
            GetCookieOptions()
        );
    }

    private void ClearRefreshCookie()
    {
        Response.Cookies.Delete(
            RefreshCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Path = RefreshCookiePath
            }
        );
    }

    // Eski sürümde Path="/" kullanıldığı için
    // bir kereye mahsus eski cookie'yi temizler.
    private void ClearLegacyCookie()
    {
        Response.Cookies.Delete(
            RefreshCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Path = "/"
            }
        );
    }

    private bool IsAllowedOrigin()
    {
        if (!Request.Headers.TryGetValue(
            "Origin",
            out var originHeader))
        {
            // Same-origin veya Origin göndermeyen istemciler.
            return true;
        }

        var origin = originHeader.ToString();

        var allowedOrigins = _configuration
            .GetSection("Auth:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        return allowedOrigins.Any(allowed =>
            string.Equals(
                allowed.TrimEnd('/'),
                origin.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase
            )
        );
    }

    private IActionResult InvalidRefresh()
    {
        return Unauthorized(new
        {
            message = "Oturum geçersiz veya süresi dolmuş."
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest loginRequest)
    {
        if (!IsAllowedOrigin())
            return StatusCode(403, new
            {
                message = "İzin verilmeyen istek kaynağı."
            });

        if (loginRequest == null ||
            string.IsNullOrWhiteSpace(loginRequest.username) ||
            string.IsNullOrWhiteSpace(loginRequest.password))
        {
            return BadRequest(new
            {
                message = "Kullanıcı adı ve şifre zorunludur."
            });
        }

        var username = loginRequest.username.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == username &&
                !u.IsDeleted);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(
                loginRequest.password,
                user.Password))
        {
            return Unauthorized(new
            {
                message = "Kullanıcı adı veya şifre hatalı."
            });
        }

        var accessToken = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var tokenHash = _jwtService.HashRefreshToken(
            refreshToken
        );

        var now = DateTime.UtcNow;

        // Mevcut tarayıcının sunduğu eski oturumu
        // iptal et; diğer cihazların oturumlarına dokunma.
        if (Request.Cookies.TryGetValue(
            RefreshCookieName,
            out var oldCookieToken) &&
            !string.IsNullOrWhiteSpace(oldCookieToken))
        {
            var oldHash = _jwtService.HashRefreshToken(
                oldCookieToken
            );

            await _context.RefreshTokens
                .Where(r =>
                    r.Token == oldHash &&
                    !r.IsRevoked)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        r => r.IsRevoked,
                        true
                    )
                );
        }

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = tokenHash,
            Created = now,
            Expires = now.AddDays(RefreshTokenDays),
            IsRevoked = false
        });

        await _context.SaveChangesAsync();

        ClearLegacyCookie();
        SetRefreshCookie(refreshToken);

        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            accessToken,
            user = new
            {
                user.Id,
                user.Username,
                user.Role
            }
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!IsAllowedOrigin())
            return StatusCode(403, new
            {
                message = "İzin verilmeyen istek kaynağı."
            });

        if (!Request.Cookies.TryGetValue(
            RefreshCookieName,
            out var rawToken) ||
            string.IsNullOrWhiteSpace(rawToken))
        {
            return InvalidRefresh();
        }

        var tokenHash = _jwtService.HashRefreshToken(
            rawToken
        );

        var now = DateTime.UtcNow;

        var currentToken = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.Token == tokenHash &&
                !r.IsRevoked &&
                r.Expires > now);

        if (currentToken == null)
            return InvalidRefresh();

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == currentToken.UserId &&
                !u.IsDeleted);

        if (user == null)
            return InvalidRefresh();

        // Aynı refresh token'ın iki kez kullanılmasını
        // engelleyen atomik güncelleme.
        int updated = await _context.RefreshTokens
            .Where(r =>
                r.Id == currentToken.Id &&
                !r.IsRevoked &&
                r.Expires > now)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    r => r.IsRevoked,
                    true
                )
            );

        if (updated == 0)
        {
            return Unauthorized(new
            {
                message = "Refresh token daha önce kullanılmış."
            });
        }

        var newRefreshToken =
            _jwtService.GenerateRefreshToken();

        var newTokenHash =
            _jwtService.HashRefreshToken(
                newRefreshToken
            );

        var newAccessToken =
            _jwtService.GenerateToken(user);

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = newTokenHash,
            Created = now,
            Expires = now.AddDays(RefreshTokenDays),
            IsRevoked = false
        });

        await _context.SaveChangesAsync();

        SetRefreshCookie(newRefreshToken);

        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            accessToken = newAccessToken
        });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        if (!IsAllowedOrigin())
            return StatusCode(403, new
            {
                message = "İzin verilmeyen istek kaynağı."
            });

        if (Request.Cookies.TryGetValue(
            RefreshCookieName,
            out var rawToken) &&
            !string.IsNullOrWhiteSpace(rawToken))
        {
            var tokenHash =
                _jwtService.HashRefreshToken(rawToken);

            await _context.RefreshTokens
                .Where(r =>
                    r.Token == tokenHash &&
                    !r.IsRevoked)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        r => r.IsRevoked,
                        true
                    )
                );
        }

        ClearRefreshCookie();
        ClearLegacyCookie();

        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            message = "Çıkış başarılı."
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new
            {
                message = "Geçersiz kullanıcı bilgisi."
            });
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == userId &&
                !u.IsDeleted);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Kullanıcı bulunamadı."
            });
        }

        Response.Headers.CacheControl = "no-store";

        // Password hash veya User entity'si döndürülmez.
        return Ok(new
        {
            user.Id,
            user.Username,
            user.Role
        });
    }
}
