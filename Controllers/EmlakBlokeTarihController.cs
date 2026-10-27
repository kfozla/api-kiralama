using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmlakBlokeTarihController : ControllerBase
{
    private readonly EmlakBlokeTarihService _service;

    public EmlakBlokeTarihController(
        EmlakBlokeTarihService service)
    {
        _service = service;
    }

    [HttpGet("emlak/{emlakId:int}")]
    public async Task<IActionResult> GetByEmlak(int emlakId)
    {
        return Ok(await _service.GetByEmlakIdAsync(emlakId));
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        EmlakBlokeTarihRequest request)
    {
        try
        {
            var result = await _service.AddAsync(request);
            return Ok(result);
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
        [FromBody] EmlakBlokeTarihRequest request)
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
        try
        {
            bool deleted = await _service.DeleteAsync(id);

            return deleted ? NoContent() : NotFound();
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

}