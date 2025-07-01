using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using onelink_tax_rule_engine.Api.Data;
using onelink_tax_rule_engine.Api.Models;   // ← Claim & ClaimType

namespace onelink_tax_rule_engine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClaimsController(AppDbContext db) => _db = db;

    // GET: api/Claims
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Claim>>> GetAll() =>
        await _db.Claims
                 .Include(c => c.ClaimType)
                 .ToListAsync();

    // GET: api/Claims/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Claim>> Get(int id)
    {
        var claim = await _db.Claims
                             .Include(c => c.ClaimType)
                             .FirstOrDefaultAsync(c => c.ClaimID == id);

        return claim is null ? NotFound() : Ok(claim);
    }
}
