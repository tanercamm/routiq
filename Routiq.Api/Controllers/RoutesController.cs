using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;
using Routiq.Api.DTOs;
using Routiq.Api.Entities;
using Routiq.Api.Services;

namespace Routiq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly IRouteGenerator _routeGenerator;
    private readonly RoutiqDbContext _context;

    public RoutesController(IRouteGenerator routeGenerator, RoutiqDbContext context)
    {
        _routeGenerator = routeGenerator;
        _context = context;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<RouteResponseDto>> GenerateRoutes([FromBody] RouteRequestDto request)
    {
        if (request.TotalBudget <= 0 || request.DurationDays <= 0)
        {
            return BadRequest("Budget and duration must be positive.");
        }

        var result = await _routeGenerator.GenerateRoutesAsync(request);
        return Ok(result);
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveRoute([FromBody] SaveRouteDto request)
    {
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == request.UserId);
        if (userProfile == null)
        {
            return NotFound("User profile not found.");
        }

        var userTrip = new UserTrip
        {
            UserProfileId = userProfile.Id,
            DestinationCityId = request.DestinationCityId,
            TotalBudget = request.TotalBudget,
            Days = request.Days,
            RouteJson = JsonSerializer.Serialize(request.RouteDetails),
            CreatedAt = DateTime.UtcNow
        };

        _context.UserTrips.Add(userTrip);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Trip saved successfully", TripId = userTrip.Id });
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<UserTripDto>>> GetUserTrips(int userId)
    {
        var userProfile = await _context.UserProfiles.Include(u => u.Trips)
                                                 .ThenInclude(t => t.DestinationCity)
                                                 .FirstOrDefaultAsync(u => u.UserId == userId);

        if (userProfile == null)
        {
            return NotFound("User profile not found.");
        }

        var trips = userProfile.Trips.Select(t => new UserTripDto
        {
            Id = t.Id,
            UserId = userId,
            DestinationCity = t.DestinationCity?.City ?? "Unknown",
            Country = t.DestinationCity?.Country ?? "Unknown",
            TotalBudget = t.TotalBudget,
            Days = t.Days,
            CreatedAt = t.CreatedAt,
            RouteDetails = string.IsNullOrEmpty(t.RouteJson) ? null : JsonSerializer.Deserialize<RouteOptionDto>(t.RouteJson)
        }).OrderByDescending(t => t.CreatedAt).ToList();

        return Ok(trips);
    }
}
