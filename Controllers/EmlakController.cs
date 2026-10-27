using api_kiralama.Models;
using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmlakController : ControllerBase
{
    private readonly EmlakService _service;
    private readonly EmlakTakvimService _takvimService;

    public EmlakController(EmlakService service, EmlakTakvimService takvimService)
    {
        _service = service;
        _takvimService = takvimService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] EmlakPagedRequest request)
    {
        var result = await _service.GetPagedAsync(request);
        return Ok(result);
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] int limit = 10)
    {
        var result = await _service.SearchAsync(
            query ?? "",
            limit
        );

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound("Emlak bulunamadı.");

        return Ok(result);
    }
    [HttpGet("{id:int}/takvim")]
    public async Task<IActionResult> GetTakvim(
        int id,
        [FromQuery] int yil,
        [FromQuery] int ay)
    {
        try
        {
            var result = await _takvimService.GetTakvimAsync(
                id, yil, ay);

            if (result == null)
                return NotFound("Emlak bulunamadı.");

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("filter")]
    public async Task<IActionResult> Filter(
        [FromQuery] int? bolgeId,
        [FromQuery] EmlakTipi? emlakTipi,
        [FromQuery] int? minKapasite,
        [FromQuery] List<int>? ozellikIds)
    {
        var result = await _service.FilterAsync(
            bolgeId, emlakTipi, minKapasite, ozellikIds);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] EmlakRequest request)
    {
        try
        {
            var result = await _service.AddAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] EmlakRequest request)
    {
        try
        {
            bool updated = await _service.UpdateAsync(id, request);

            if (!updated)
                return NotFound("Emlak bulunamadı.");

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound("Emlak bulunamadı.");

        return NoContent();
    }
    [HttpPatch("{id:int}/yayin-durumu")]
    [Authorize]
    public async Task<IActionResult> UpdateYayinDurumu(
        int id,
        [FromBody] EmlakYayinDurumuRequest request)
    {
        var result = await _service.UpdateYayinDurumuAsync(id, request.Yayinda);

        if (!result)
            return NotFound(new { message = "Emlak bulunamadı." });

        return Ok(new
        {
            message = request.Yayinda
                ? "Emlak yayına alındı."
                : "Emlak yayından kaldırıldı.",
            yayinda = request.Yayinda
        });
    }
}