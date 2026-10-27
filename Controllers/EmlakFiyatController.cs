using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmlakFiyatController : ControllerBase
{
    private readonly EmlakFiyatService _service;

    public EmlakFiyatController(EmlakFiyatService service)
    {
        _service = service;
    }

    [HttpGet("emlak/{emlakId:int}")]
    public async Task<IActionResult> GetByEmlak(int emlakId)
    {
        return Ok(await _service.GetByEmlakIdAsync(emlakId));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var fiyat = await _service.GetByIdAsync(id);
        return fiyat == null ? NotFound() : Ok(fiyat);
    }

    [HttpPost]
    public async Task<IActionResult> Add(EmlakFiyatRequest request)
    {
        try
        {
            var result = await _service.AddAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, EmlakFiyatRequest request)
    {
        try
        {
            bool updated = await _service.UpdateAsync(id, request);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}