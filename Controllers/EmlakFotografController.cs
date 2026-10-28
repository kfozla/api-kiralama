
using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/Emlak/{emlakId:int}/fotograflar")]
[Authorize]
public class EmlakFotografController : ControllerBase
{
    //private readonly EmlakFotografService _service;
    private readonly EmlakFotografS3Service _service;

    public EmlakFotografController(
        EmlakFotografS3Service service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int emlakId)
    {
        try
        {
            var result = await _service.GetAsync(emlakId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(210_000_000)]
    public async Task<IActionResult> Upload(
        int emlakId,
        [FromForm] EmlakFotografUploadRequest request)
    {
        try
        {
            var result = await _service.UploadAsync(
                emlakId,
                request.Files
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

    [HttpDelete("{fotografId:int}")]
    public async Task<IActionResult> Delete(
        int emlakId,
        int fotografId)
    {
        try
        {
            await _service.DeleteAsync(
                emlakId,
                fotografId
            );

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{fotografId:int}/kapak")]
    public async Task<IActionResult> SetCover(
        int emlakId,
        int fotografId)
    {
        try
        {
            await _service.SetCoverAsync(
                emlakId,
                fotografId
            );

            return Ok(new
            {
                message = "Kapak fotoğrafı güncellendi."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("siralama")]
    public async Task<IActionResult> Reorder(
        int emlakId,
        [FromBody] EmlakFotografSiralamaRequest request)
    {
        try
        {
            await _service.ReorderAsync(
                emlakId,
                request.FotografIds
            );

            return Ok(new
            {
                message = "Fotoğraf sıralaması güncellendi."
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
}
