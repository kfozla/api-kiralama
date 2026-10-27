
using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnasayfaBolumController : ControllerBase
{
    private readonly AnasayfaBolumService _service;

    public AnasayfaBolumController(
        AnasayfaBolumService service)
    {
        _service = service;
    }

    // GET /api/AnasayfaBolum
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(result);
    }

    // GET /api/AnasayfaBolum/public
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic()
    {
        var result = await _service.GetPublicAsync();

        return Ok(result);
    }

    // GET /api/AnasayfaBolum/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound(new
            {
                message = "Anasayfa bölümü bulunamadı."
            });

        return Ok(result);
    }

    // POST /api/AnasayfaBolum
    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] AnasayfaBolumRequest request)
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
    }

    // PUT /api/AnasayfaBolum/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] AnasayfaBolumRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);

            if (!result)
                return NotFound(new
                {
                    message = "Anasayfa bölümü bulunamadı."
                });

            return Ok(new
            {
                message = "Anasayfa bölümü güncellendi."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/AnasayfaBolum/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
            return NotFound(new
            {
                message = "Anasayfa bölümü bulunamadı."
            });

        return Ok(new
        {
            message = "Anasayfa bölümü silindi."
        });
    }

    // PUT /api/AnasayfaBolum/5/emlaklar
    [HttpPut("{id:int}/emlaklar")]
    public async Task<IActionResult> SaveEmlaklar(
        int id,
        [FromBody] AnasayfaBolumEmlaklarRequest request)
    {
        try
        {
            var result = await _service.SaveEmlaklarAsync(
                id, request);

            if (!result)
                return NotFound(new
                {
                    message = "Anasayfa bölümü bulunamadı."
                });

            return Ok(new
            {
                message = "Bölümün villaları ve sıralaması kaydedildi."
            });
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
