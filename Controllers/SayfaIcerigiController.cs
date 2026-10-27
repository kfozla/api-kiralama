using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SayfaIcerigiController : ControllerBase
{
    private readonly SayfaIcerigiService _service;

    public SayfaIcerigiController(
        SayfaIcerigiService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var data = await _service.GetBySlugAsync(slug);

        if (data == null)
            return NotFound();

        return Ok(data);
    }

    [HttpPut("{slug}")]
    [Authorize]
    public async Task<IActionResult> Update(
        string slug,
        [FromBody] SayfaIcerigiUpdateRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(
                slug, request
            );

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}