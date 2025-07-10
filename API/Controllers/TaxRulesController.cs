using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OklahomaTaxEngine.Data;
using OklahomaTaxEngine.Models;
using OklahomaTaxEngine.Services;

namespace OklahomaTaxEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxRulesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IRuleEngine _ruleEngine;
        private readonly ILogger<TaxRulesController> _logger;

        public TaxRulesController(
            AppDbContext context, 
            IRuleEngine ruleEngine,
            ILogger<TaxRulesController> logger)
        {
            _context = context;
            _ruleEngine = ruleEngine;
            _logger = logger;
        }

        // GET: api/taxrules
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaxRule>>> GetTaxRules(
            [FromQuery] string taxType = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] DateTime? effectiveDate = null)
        {
            var query = _context.TaxRules.AsQueryable();

            if (!string.IsNullOrEmpty(taxType))
                query = query.Where(r => r.TaxType == taxType);

            if (isActive.HasValue)
                query = query.Where(r => r.IsActive == isActive.Value);

            if (effectiveDate.HasValue)
            {
                query = query.Where(r => r.EffectiveFrom <= effectiveDate.Value &&
                                       (r.EffectiveTo == null || r.EffectiveTo >= effectiveDate.Value));
            }

            return await query
                .OrderBy(r => r.TaxType)
                .ThenBy(r => r.Priority)
                .ThenBy(r => r.MinAmount)
                .ToListAsync();
        }

        // GET: api/taxrules/active?taxType=Income&date=2024-01-01
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TaxRule>>> GetActiveRules(
            [FromQuery] string taxType,
            [FromQuery] DateTime? date = null)
        {
            if (string.IsNullOrEmpty(taxType))
            {
                return BadRequest(new { error = "Tax type is required" });
            }

            var effectiveDate = date ?? DateTime.UtcNow;
            var rules = await _ruleEngine.GetApplicableRulesAsync(taxType, effectiveDate);

            return Ok(rules);
        }

        // GET: api/taxrules/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaxRule>> GetTaxRule(int id)
        {
            var taxRule = await _context.TaxRules.FindAsync(id);

            if (taxRule == null)
            {
                return NotFound();
            }

            return taxRule;
        }

        // POST: api/taxrules
        [HttpPost]
        public async Task<ActionResult<TaxRule>> CreateTaxRule(TaxRule taxRule)
        {
            // Validate tax type
            if (!TaxTypes.IsValid(taxRule.TaxType))
            {
                return BadRequest(new { error = "Invalid tax type" });
            }

            // Validate rate
            if (taxRule.Rate < 0 || taxRule.Rate > 1)
            {
                return BadRequest(new { error = "Rate must be between 0 and 1" });
            }

            // Validate amount range
            if (taxRule.MinAmount.HasValue && taxRule.MaxAmount.HasValue && 
                taxRule.MinAmount > taxRule.MaxAmount)
            {
                return BadRequest(new { error = "MinAmount cannot be greater than MaxAmount" });
            }

            _context.TaxRules.Add(taxRule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new tax rule: {RuleName} for {TaxType}", 
                taxRule.RuleName, taxRule.TaxType);

            return CreatedAtAction(nameof(GetTaxRule), new { id = taxRule.Id }, taxRule);
        }

        // PUT: api/taxrules/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaxRule(int id, TaxRule taxRule)
        {
            if (id != taxRule.Id)
            {
                return BadRequest();
            }

            var existingRule = await _context.TaxRules.FindAsync(id);
            if (existingRule == null)
            {
                return NotFound();
            }

            _context.Entry(existingRule).CurrentValues.SetValues(taxRule);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated tax rule: {RuleName}", taxRule.RuleName);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxRuleExists(id))
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

        // DELETE: api/taxrules/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaxRule(int id)
        {
            var taxRule = await _context.TaxRules.FindAsync(id);
            if (taxRule == null)
            {
                return NotFound();
            }

            // Soft delete
            taxRule.IsActive = false;
            taxRule.EffectiveTo = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deactivated tax rule: {RuleName}", taxRule.RuleName);

            return NoContent();
        }

        private bool TaxRuleExists(int id)
        {
            return _context.TaxRules.Any(e => e.Id == id);
        }
    }
}