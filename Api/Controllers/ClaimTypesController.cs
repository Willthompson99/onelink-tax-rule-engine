using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using onelink_tax_rule_engine.Api.Data;
using onelink_tax_rule_engine.Api.Models;

namespace onelink_tax_rule_engine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimTypesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClaimType>>> Get() =>
        await db.ClaimTypes.ToListAsync();
}
