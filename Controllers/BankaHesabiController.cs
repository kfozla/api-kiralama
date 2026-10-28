
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankaHesabiController : ControllerBase
{
    private readonly BankaHesabiS3Service _service;

    public BankaHesabiController(BankaHesabiS3Service service)
    {
        _service = service;
    }

    private object ToResponse(BankaHesabi banka)
    {
        return new
        {
            banka.Id,
            banka.BankaAdi,
            banka.HesapSahibi,
            banka.Iban,
            banka.SubeAdi,
            banka.ParaBirimi,
            banka.Sira,
            banka.AktifMi,
            banka.CreatedAt,
            banka.UpdatedAt,
            LogoUrl = _service.GetLogoUrl(banka.LogoUrl)
        };
    }

    // GET /api/BankaHesabi
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(result.Select(ToResponse).ToList());
    }

    // GET /api/BankaHesabi/admin
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetForAdmin()
    {
        var result = await _service.GetAllAsync(true);

        return Ok(result.Select(ToResponse).ToList());
    }

    // GET /api/BankaHesabi/5
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        return result == null
            ? NotFound()
            : Ok(ToResponse(result));
    }

    // POST /api/BankaHesabi
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] BankaHesabiRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);

            return Ok(ToResponse(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // PUT /api/BankaHesabi/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] BankaHesabiRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);

            return result == null
                ? NotFound()
                : Ok(ToResponse(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE /api/BankaHesabi/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }

    // POST /api/BankaHesabi/5/logo
    [HttpPost("{id:int}/logo")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024 + 65536)]
    public async Task<IActionResult> UploadLogo(
        int id,
        [FromForm] IFormFile file)
    {
        try
        {
            var result = await _service.UploadLogoAsync(id, file);

            return result == null
                ? NotFound()
                : Ok(ToResponse(result));
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLogo(int id)
    {
        var result = await _service.DeleteLogoAsync(id);

        return result == null
            ? NotFound()
            : Ok(ToResponse(result));
    }
}
