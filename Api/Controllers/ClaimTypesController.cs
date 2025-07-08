using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using onelink_tax_rule_engine.Api.Data;
using onelink_tax_rule_engine.Api.Models;

namespace onelink_tax_rule_engine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimTypesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClaimTypesController(AppDbContext db) => _db = db;

    // GET api/claimtypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClaimType>>> GetAll() =>
        await _db.ClaimTypes.AsNoTracking().ToListAsync();

    // POST api/claimtypes
    [HttpPost]
    public async Task<ActionResult<ClaimType>> Create(ClaimType body)
    {
        _db.ClaimTypes.Add(body);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = body.ClaimTypeID }, body);
    }

    // GET api/claimtypes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClaimType>> GetById(int id)
    {
        var row = await _db.ClaimTypes.FindAsync(id);
        return row is null ? NotFound() : Ok(row);
    }

    // PUT api/claimtypes/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ClaimType body)
    {
        if (id != body.ClaimTypeID) return BadRequest();
        _db.Entry(body).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/claimtypes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await _db.ClaimTypes.FindAsync(id);
        if (row is null) return NotFound();
        _db.Remove(row);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
