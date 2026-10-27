using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BolgeController : ControllerBase
{
    private readonly BolgeService _service;

    public BolgeController(BolgeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound("Bölge bulunamadı.");

        return Ok(result);
    }

    [HttpGet("alt-bolgeler")]
    public async Task<IActionResult> GetAltBolgeler(
        [FromQuery] int? parentId)
    {
        if (parentId.HasValue)
        {
            var parent = await _service.GetByIdAsync(parentId.Value);

            if (parent == null)
                return NotFound("Üst bölge bulunamadı.");
        }

        var result = await _service.GetAltBolgelerAsync(parentId);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] BolgeRequest request)
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] BolgeRequest request)
    {
        try
        {
            bool updated = await _service.UpdateAsync(id, request);

            if (!updated)
                return NotFound("Bölge bulunamadı.");

            return NoContent();
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
        try
        {
            bool deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound("Bölge bulunamadı.");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}