using api_kiralama.Models;
using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmlakOzellikController : ControllerBase
{
    //private readonly EmlakOzellikService _service;
    private readonly EmlakOzellikS3Service _service;

    public EmlakOzellikController(EmlakOzellikS3Service service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound("Özellik bulunamadı.");

        return Ok(ToResponse(result));
    }

    [HttpGet("kategori/{kategori}")]
    public async Task<IActionResult> GetByKategori(
        OzellikKategori kategori)
    {
        if (!Enum.IsDefined(typeof(OzellikKategori), kategori))
            return BadRequest("Geçersiz kategori.");

        var result = await _service.GetByKategoriAsync(kategori);

        return Ok(result.Select(ToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] EmlakOzellikRequest request)
    {
        try
        {
            var result = await _service.AddAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                ToResponse(result)
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
        [FromBody] EmlakOzellikRequest request)
    {
        try
        {
            bool updated = await _service.UpdateAsync(id, request);

            if (!updated)
                return NotFound("Özellik bulunamadı.");

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
                return NotFound("Özellik bulunamadı.");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private object ToResponse(EmlakOzellik ozellik)
    {
        return new
        {
            ozellik.Id,
            ozellik.OzellikAdi,
            ozellik.Kategori,
            //ozellik.LogoUrl
            LogoUrl = _service.GetLogoUrl(ozellik.LogoUrl)
        };
    }
    
    
    [HttpPost("{id:int}/logo")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadLogo(
        int id,
        IFormFile file)
    {
        try
        {
            var logoUrl = await _service
                .UploadLogoAsync(id, file);

            return Ok(new
            {
                message = "Logo başarıyla yüklendi.",
                logoUrl
            });
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

    [HttpDelete("{id:int}/logo")]
    [Authorize]
    public async Task<IActionResult> DeleteLogo(int id)
    {
        var result = await _service
            .DeleteLogoAsync(id);

        if (!result)
            return NotFound(new
            {
                message = "Emlak özelliği bulunamadı."
            });

        return Ok(new
        {
            message = "Logo kaldırıldı.",
            logoUrl = (string?)null
        });
    }

}