using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_kiralama.Controllers;

[ApiController]
[Route("api/Dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();

        return Ok(summary);
    }
    
    [HttpGet("reservations/last-six-months")]
    public async Task<IActionResult> GetLastSixMonthsReservations()
    {
        var result = await _dashboardService
            .GetLastSixMonthsReservationsAsync();

        return Ok(result);
    }
    
    [HttpGet("recent-reservations")]
    public async Task<IActionResult> GetRecentReservations()
    {
        var reservations = await _dashboardService
            .GetRecentReservationsAsync();

        return Ok(reservations);
    }


}