
using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankaHesabiController : ControllerBase
{
    private readonly BankaHesabiService _service;

    public BankaHesabiController(BankaHesabiService service)
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

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] BankaHesabiRequest request)
    {
        try
        {
            return Ok(await _service.CreateAsync(request));
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
        [FromBody] BankaHesabiRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);

            return result == null ? NotFound() : Ok(result);
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

   
    [HttpPost("{id:int}/logo")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024 + 65536)]
    public async Task<IActionResult> UploadLogo(
        int id,
        IFormFile file)
    {
        try
        {
            var result = await _service.UploadLogoAsync(id, file);

            return result == null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // DELETE /api/BankaHesabi/5/logo
    [HttpDelete("{id:int}/logo")]
    [Authorize]
    public async Task<IActionResult> DeleteLogo(int id)
    {
        var result = await _service.DeleteLogoAsync(id);

        return result == null ? NotFound() : Ok(result);
    }
}
