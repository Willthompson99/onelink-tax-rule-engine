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
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITaxCalculationService _calculationService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            AppDbContext context,
            ITaxCalculationService calculationService,
            ILogger<TransactionsController> logger)
        {
            _context = context;
            _calculationService = calculationService;
            _logger = logger;
        }

        // GET: api/transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaxTransaction>>> GetTransactions(
            [FromQuery] string status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.TaxTransactions
                .Include(t => t.TaxPayer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            var totalCount = await query.CountAsync();

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.TransactionId,
                    t.TaxPayerId,
                    TaxPayerName = t.TaxPayer.Name,
                    TaxPayerTaxId = t.TaxPayer.TaxId,
                    t.TaxType,
                    t.TaxableAmount,
                    t.TaxAmount,
                    t.TotalAmount,
                    t.TransactionDate,
                    t.TaxPeriod,
                    t.Status,
                    t.PaymentMethod,
                    t.PaymentDate,
                    t.Notes
                })
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(transactions);
        }

        // GET: api/transactions/taxpayer/OK123456789
        [HttpGet("taxpayer/{taxId}")]
        public async Task<ActionResult<IEnumerable<TaxTransaction>>> GetTaxPayerTransactions(string taxId)
        {
            var taxpayer = await _context.TaxPayers
                .FirstOrDefaultAsync(t => t.TaxId == taxId);

            if (taxpayer == null)
            {
                return NotFound(new { error = "Taxpayer not found" });
            }

            var transactions = await _context.TaxTransactions
                .Where(t => t.TaxPayerId == taxpayer.Id)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return Ok(transactions);
        }

        // GET: api/transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaxTransaction>> GetTransaction(int id)
        {
            var transaction = await _context.TaxTransactions
                .Include(t => t.TaxPayer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        // POST: api/transactions/calculate
        [HttpPost("calculate")]
        public async Task<ActionResult<TaxCalculationResponse>> CalculateTax(TaxCalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _calculationService.CalculateTaxAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Message });
            }

            return Ok(result);
        }

        // POST: api/transactions
        [HttpPost]
        public async Task<ActionResult<TaxTransaction>> CreateTransaction(TaxCalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var transaction = await _calculationService.CreateTransactionAsync(request);
                
                _logger.LogInformation("Created transaction: {TransactionId}", transaction.TransactionId);

                return CreatedAtAction(nameof(GetTransaction), 
                    new { id = transaction.Id }, transaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/transactions/5/pay
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> PayTransaction(int id, [FromBody] PaymentRequest payment)
        {
            var transaction = await _context.TaxTransactions.FindAsync(id);
            
            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Status != "Pending")
            {
                return BadRequest(new { error = "Transaction is not in pending status" });
            }

            transaction.MarkAsPaid(payment.PaymentMethod);
            transaction.Notes = $"{transaction.Notes} | Paid on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction {TransactionId} marked as paid", transaction.TransactionId);

            return NoContent();
        }

        // PUT: api/transactions/5/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelTransaction(int id)
        {
            var transaction = await _context.TaxTransactions.FindAsync(id);
            
            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Status != "Pending")
            {
                return BadRequest(new { error = "Only pending transactions can be cancelled" });
            }

            transaction.Status = "Cancelled";
            transaction.Notes = $"{transaction.Notes} | Cancelled on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction {TransactionId} cancelled", transaction.TransactionId);

            return NoContent();
        }
    }

    public class PaymentRequest
    {
        public string PaymentMethod { get; set; }
    }
}