using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Routiq.Api.Data;
using Routiq.Api.Entities;

namespace Routiq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunityController : ControllerBase
{
    private readonly RoutiqDbContext _context;

    public CommunityController(RoutiqDbContext context)
    {
        _context = context;
    }

    [HttpPost("tips/{id}/like")]
    public async Task<IActionResult> LikeTip(Guid id)
    {
        var tip = await _context.DestinationTips.FindAsync(id);

        if (tip == null)
        {
            return NotFound();
        }

        tip.Upvotes++;
        await _context.SaveChangesAsync();

        return Ok(new { tip.Id, tip.Upvotes });
    }
}
