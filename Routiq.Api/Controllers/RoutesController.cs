using Microsoft.AspNetCore.Mvc;
using Routiq.Api.DTOs;
using Routiq.Api.Services;

namespace Routiq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly IRouteGenerator _routeGenerator;

    public RoutesController(IRouteGenerator routeGenerator)
    {
        _routeGenerator = routeGenerator;
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
}
