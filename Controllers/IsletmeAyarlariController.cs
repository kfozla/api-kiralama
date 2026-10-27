
using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/IsletmeAyarlari")]
[Authorize]
public class IsletmeAyarlariController : ControllerBase
{
    private readonly IsletmeAyarlariService _service;

    public IsletmeAyarlariController(
        IsletmeAyarlariService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _service.GetAsync());
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        [FromBody] IsletmeAyarlariRequest request)
    {
        try
        {
            return Ok(await _service.UpdateAsync(request));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    
    [HttpPost("logo")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        try
        {
            return Ok(await _service.UploadLogoAsync(file));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("logo")]
    public async Task<IActionResult> GetLogo()
    {
        var file = await _service.GetFileAsync(true);

        if (file == null)
            return NotFound();

        Response.Headers.CacheControl = "private, no-store";

        return File(file.Value.Data, file.Value.ContentType);
    }

    [HttpDelete("logo")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLogo()
    {
        bool deleted = await _service.DeleteLogoAsync();

        return deleted ? NoContent() : NotFound();
    }

    
    [HttpPost("tursab-belgesi")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> UploadTursab(IFormFile file)
    {
        try
        {
            return Ok(await _service.UploadTursabAsync(file));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("tursab-belgesi")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTursab()
    {
        var file = await _service.GetFileAsync(false);

        if (file == null)
            return NotFound();

        Response.Headers.CacheControl = "private, no-store";

        return File(file.Value.Data, file.Value.ContentType);
    }

    [HttpDelete("tursab-belgesi")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTursab()
    {
        bool deleted = await _service.DeleteTursabAsync();

        return deleted ? NoContent() : NotFound();
    }
}
