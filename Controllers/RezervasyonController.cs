using api_kiralama.Models;
using api_kiralama.Dtos;
using api_kiralama.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RezervasyonController : ControllerBase
{
    private readonly RezervasyonService _service;

    public RezervasyonController(RezervasyonService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound("Rezervasyon bulunamadı.");

        return Ok(result);
    }
    
    
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] RezervasyonPagedRequest request)
    {
        try
        {
            var result = await _service
                .GetPagedAsync(request);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
    
    [HttpGet("takvim")]
    public async Task<IActionResult> GetTakvim(
        [FromQuery] RezervasyonTakvimRequest request)
    {
        try
        {
            var result = await _service
                .GetTakvimAsync(request);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    
    
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _service.GetStatsAsync();

        return Ok(result);
    }



    [HttpGet("emlak/{emlakId:int}")]
    public async Task<IActionResult> GetByEmlak(int emlakId)
    {
        return Ok(await _service.GetByEmlakIdAsync(emlakId));
    }

    [HttpGet("musaitlik")]
    public async Task<IActionResult> CheckAvailability(
        [FromQuery] int emlakId,
        [FromQuery] DateOnly giris,
        [FromQuery] DateOnly cikis)
    {
        try
        {
            bool musait = await _service.CheckAvailabilityAsync(
                emlakId, giris, cikis);

            return Ok(new { emlakId, musait });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Add(RezervasyonRequest request)
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] RezervasyonRequest request)
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

    [HttpPatch("{id:int}/durum")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        RezervasyonDurumRequest request)
    {
        try
        {
            bool updated = await _service.ChangeStatusAsync(
                id, request.Durum);

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

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Rezervasyon bulunamadı."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

}