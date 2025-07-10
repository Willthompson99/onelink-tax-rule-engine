using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OklahomaTaxEngine.Data;
using OklahomaTaxEngine.Models;

namespace OklahomaTaxEngine.Services
{
    public interface ITaxCalculationService
    {
        Task<TaxCalculationResponse> CalculateTaxAsync(TaxCalculationRequest request);
        Task<TaxTransaction> CreateTransactionAsync(TaxCalculationRequest request);
    }

    public class TaxCalculationService : ITaxCalculationService
    {
        private readonly AppDbContext _context;
        private readonly IRuleEngine _ruleEngine;
        private readonly ILogger<TaxCalculationService> _logger;

        public TaxCalculationService(
            AppDbContext context, 
            IRuleEngine ruleEngine, 
            ILogger<TaxCalculationService> logger)
        {
            _context = context;
            _ruleEngine = ruleEngine;
            _logger = logger;
        }

        public async Task<TaxCalculationResponse> CalculateTaxAsync(TaxCalculationRequest request)
        {
            try
            {
                // Validate tax type
                if (!TaxTypes.IsValid(request.TaxType))
                {
                    return new TaxCalculationResponse
                    {
                        Success = false,
                        Message = $"Invalid tax type: {request.TaxType}"
                    };
                }

                // Validate taxpayer exists
                var taxpayer = await _context.TaxPayers
                    .FirstOrDefaultAsync(t => t.TaxId == request.TaxPayerId && t.IsActive);

                if (taxpayer == null)
                {
                    return new TaxCalculationResponse
                    {
                        Success = false,
                        Message = $"Taxpayer not found: {request.TaxPayerId}"
                    };
                }

                var calculationDate = request.CalculationDate ?? DateTime.UtcNow;
                var appliedRules = await _ruleEngine.ApplyRulesAsync(
                    request.TaxType, 
                    request.TaxableAmount, 
                    calculationDate);

                var totalTax = appliedRules.Sum(r => r.CalculatedAmount);
                var totalAmount = request.TaxableAmount + totalTax;

                _logger.LogInformation(
                    "Calculated tax for {TaxPayerId}: Type={TaxType}, Taxable={TaxableAmount}, Tax={TaxAmount}, Total={TotalAmount}",
                    request.TaxPayerId, request.TaxType, request.TaxableAmount, totalTax, totalAmount);

                return new TaxCalculationResponse
                {
                    Success = true,
                    Message = "Tax calculated successfully",
                    TaxableAmount = request.TaxableAmount,
                    TaxAmount = totalTax,
                    TotalAmount = totalAmount,
                    AppliedRules = appliedRules
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating tax for {TaxPayerId}", request.TaxPayerId);
                return new TaxCalculationResponse
                {
                    Success = false,
                    Message = "An error occurred while calculating tax"
                };
            }
        }

        public async Task<TaxTransaction> CreateTransactionAsync(TaxCalculationRequest request)
        {
            var calculationResult = await CalculateTaxAsync(request);
            
            if (!calculationResult.Success)
            {
                throw new InvalidOperationException(calculationResult.Message);
            }

            var taxpayer = await _context.TaxPayers
                .FirstAsync(t => t.TaxId == request.TaxPayerId);

            var transaction = new TaxTransaction
            {
                TransactionId = GenerateTransactionId(),
                TaxPayerId = taxpayer.Id,
                TaxType = request.TaxType,
                TaxableAmount = calculationResult.TaxableAmount,
                TaxAmount = calculationResult.TaxAmount,
                TotalAmount = calculationResult.TotalAmount,
                TransactionDate = DateTime.UtcNow,
                TaxPeriod = request.TaxPeriod ?? GetCurrentTaxPeriod(request.TaxType),
                Status = "Pending",
                Notes = $"Applied rules: {string.Join(", ", calculationResult.AppliedRules.Select(r => r.RuleName))}"
            };

            _context.TaxTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created transaction {TransactionId} for {TaxPayerId}: Amount={TotalAmount}",
                transaction.TransactionId, request.TaxPayerId, transaction.TotalAmount);

            return transaction;
        }

        private string GenerateTransactionId()
        {
            return $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        private string GetCurrentTaxPeriod(string taxType)
        {
            var now = DateTime.UtcNow;
            
            return taxType switch
            {
                TaxTypes.Income => $"{now.Year}",
                TaxTypes.Sales => $"{now.Year}-{now.Month:D2}",
                TaxTypes.Property => $"{now.Year}",
                TaxTypes.Corporate => $"{now.Year}-Q{(now.Month - 1) / 3 + 1}",
                _ => $"{now.Year}-{now.Month:D2}"
            };
        }
    }
}