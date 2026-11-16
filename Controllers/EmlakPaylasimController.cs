using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmlakPaylasimController : ControllerBase
{
    private readonly EmlakPaylasimService _service;

    public EmlakPaylasimController(
        EmlakPaylasimService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Olustur(
        [FromBody] EmlakPaylasimOlusturRequest request)
    {
        try
        {
            var response = await _service.OlusturAsync(request);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("public/{token}")]
    public async Task<IActionResult> GetPublic(string token)
    {
        var response = await _service.GetPublicAsync(token);

        if (response == null)
        {
            return NotFound(new
            {
                message = "Paylaşım bulunamadı veya süresi dolmuş."
            });
        }

        Response.Headers.CacheControl = "no-store";

        return Ok(response);
    }
}