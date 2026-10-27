using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SikcaSorulanSoruController : ControllerBase
{
    private readonly SikcaSorulanSoruService _service;

    public SikcaSorulanSoruController(
        SikcaSorulanSoruService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("admin")]
    [Authorize]
    public async Task<IActionResult> GetForAdmin()
    {
        return Ok(await _service.GetAllAsync(true));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] SikcaSorulanSoruRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] SikcaSorulanSoruRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}