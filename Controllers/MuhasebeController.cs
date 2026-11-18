using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MuhasebeController : ControllerBase
{
    private readonly MuhasebeService _service;

    public MuhasebeController(MuhasebeService service)
    {
        _service = service;
    }

    [HttpGet("rezervasyonlar")]
    public async Task<IActionResult> GetRezervasyonlar(
        [FromQuery] MuhasebePagedRequest request)
    {
        var result = await _service.GetPagedAsync(request);

        return Ok(result);
    }
    
    [HttpGet("ozet")]
    public async Task<IActionResult> GetOzet(
        [FromQuery] MuhasebeOzetRequest request)
    {
        if (request.BaslangicTarihi.HasValue &&
            request.BitisTarihi.HasValue &&
            request.BitisTarihi.Value < request.BaslangicTarihi.Value)
        {
            return BadRequest(new
            {
                message = "Bitiş tarihi başlangıç tarihinden önce olamaz."
            });
        }

        var result = await _service.GetOzetAsync(request);

        return Ok(result);
    }
    [HttpGet("aylik-grafik")]
    public async Task<IActionResult> GetAylikGrafik(
        [FromQuery] MuhasebeOzetRequest request)
    {
        if (request.BaslangicTarihi.HasValue &&
            request.BitisTarihi.HasValue &&
            request.BitisTarihi.Value < request.BaslangicTarihi.Value)
            return BadRequest(new { message = "Bitiş tarihi başlangıç tarihinden önce olamaz." });

        return Ok(await _service.GetAylikGrafikAsync(request));
    }


}