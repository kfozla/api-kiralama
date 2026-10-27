using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmlakSahibiController : ControllerBase
{
    private readonly EmlakSahibiService _service;
    private readonly EmlakService _emlakService;

    public EmlakSahibiController(EmlakSahibiService service, EmlakService emlakService)
    {
        _service = service;
        _emlakService = emlakService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
    
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] EmlakSahibiPagedRequest request)
    {
        var result = await _service.GetPagedAsync(request);

        return Ok(result);
    }

    
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) ||
            query.Trim().Length < 2)
        {
            return Ok(new List<EmlakSahibiSearchResponse>());
        }

        var result = await _service.SearchAsync(
            query,
            limit
        );

        return Ok(result);
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound("Emlak sahibi bulunamadı.");

        return Ok(result);
    }
    [HttpGet("{id:int}/emlaklar")]
    public async Task<IActionResult> GetEmlaklar(int id)
    {
        var emlakSahibi = await _service.GetByIdAsync(id);

        if (emlakSahibi == null)
            return NotFound("Emlak sahibi bulunamadı.");

        var emlaklar = await _emlakService.GetByEmlakSahibiIdAsync(id);

        return Ok(emlaklar);
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] EmlakSahibiRequest request)
    {
        var result = await _service.AddAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] EmlakSahibiRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);

        if (!updated)
            return NotFound("Emlak sahibi bulunamadı.");

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound("Emlak sahibi bulunamadı.");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}