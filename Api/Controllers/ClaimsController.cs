using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using onelink_tax_rule_engine.Api.Data;
using onelink_tax_rule_engine.Api.Models;

namespace onelink_tax_rule_engine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController(AppDbContext db) : ControllerBase
{
    // GET: api/Claims
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Claim>>> GetAll() =>
        await db.Claims
                .Include(c => c.ClaimType)
                .ToListAsync();

    // GET: api/Claims/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Claim>> Get(int id)
    {
        var claim = await db.Claims
                            .Include(c => c.ClaimType)
                            .FirstOrDefaultAsync(c => c.ClaimID == id);

        return claim is null ? NotFound() : Ok(claim);
    }

    // POST: api/Claims
    [HttpPost]
    public async Task<ActionResult<Claim>> Post(Claim dto)
    {
        db.Claims.Add(dto);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = dto.ClaimID }, dto);
    }

    // PUT: api/Claims/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Claim dto)
    {
        if (id != dto.ClaimID) return BadRequest();
        db.Entry(dto).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Claims/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var claim = await db.Claims.FindAsync(id);
        if (claim is null) return NotFound();

        db.Remove(claim);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
