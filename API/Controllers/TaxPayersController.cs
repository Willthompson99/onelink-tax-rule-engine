using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OklahomaTaxEngine.Data;
using OklahomaTaxEngine.Models;

namespace OklahomaTaxEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxPayersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaxPayersController> _logger;

        public TaxPayersController(AppDbContext context, ILogger<TaxPayersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/taxpayers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaxPayer>>> GetTaxPayers(
            [FromQuery] string type = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.TaxPayers.AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.Type == type);

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();
            
            var taxpayers = await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return taxpayers;
        }

        // GET: api/taxpayers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaxPayer>> GetTaxPayer(int id)
        {
            var taxPayer = await _context.TaxPayers.FindAsync(id);

            if (taxPayer == null)
            {
                return NotFound();
            }

            return taxPayer;
        }

        // GET: api/taxpayers/bytaxid/OK123456789
        [HttpGet("bytaxid/{taxId}")]
        public async Task<ActionResult<TaxPayer>> GetTaxPayerByTaxId(string taxId)
        {
            var taxPayer = await _context.TaxPayers
                .FirstOrDefaultAsync(t => t.TaxId == taxId);

            if (taxPayer == null)
            {
                return NotFound();
            }

            return taxPayer;
        }

        // POST: api/taxpayers
        [HttpPost]
        public async Task<ActionResult<TaxPayer>> CreateTaxPayer(TaxPayer taxPayer)
        {
            // Validate unique TaxId
            if (await _context.TaxPayers.AnyAsync(t => t.TaxId == taxPayer.TaxId))
            {
                return BadRequest(new { error = "Tax ID already exists" });
            }

            // Validate tax type
            var validTypes = new[] { "Individual", "Corporate", "Partnership", "Trust", "Estate" };
            if (!validTypes.Contains(taxPayer.Type))
            {
                return BadRequest(new { error = "Invalid taxpayer type" });
            }

            taxPayer.RegistrationDate = DateTime.UtcNow;
            taxPayer.IsActive = true;

            _context.TaxPayers.Add(taxPayer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new taxpayer: {TaxId} - {Name}", taxPayer.TaxId, taxPayer.Name);

            return CreatedAtAction(nameof(GetTaxPayer), new { id = taxPayer.Id }, taxPayer);
        }

        // PUT: api/taxpayers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaxPayer(int id, TaxPayer taxPayer)
        {
            if (id != taxPayer.Id)
            {
                return BadRequest();
            }

            var existingTaxPayer = await _context.TaxPayers.FindAsync(id);
            if (existingTaxPayer == null)
            {
                return NotFound();
            }

            // Don't allow changing TaxId
            if (existingTaxPayer.TaxId != taxPayer.TaxId)
            {
                return BadRequest(new { error = "Cannot change Tax ID" });
            }

            _context.Entry(existingTaxPayer).CurrentValues.SetValues(taxPayer);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated taxpayer: {TaxId}", taxPayer.TaxId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxPayerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/taxpayers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaxPayer(int id)
        {
            var taxPayer = await _context.TaxPayers.FindAsync(id);
            if (taxPayer == null)
            {
                return NotFound();
            }

            // Soft delete - just mark as inactive
            taxPayer.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deactivated taxpayer: {TaxId}", taxPayer.TaxId);

            return NoContent();
        }

        private bool TaxPayerExists(int id)
        {
            return _context.TaxPayers.Any(e => e.Id == id);
        }
    }
}