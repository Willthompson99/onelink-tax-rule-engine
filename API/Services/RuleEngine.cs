using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OklahomaTaxEngine.Data;
using OklahomaTaxEngine.Models;

namespace OklahomaTaxEngine.Services
{
    public interface IRuleEngine
    {
        Task<List<TaxRule>> GetApplicableRulesAsync(string taxType, DateTime effectiveDate);
        Task<List<RuleApplication>> ApplyRulesAsync(string taxType, decimal amount, DateTime effectiveDate);
    }

    public class RuleEngine : IRuleEngine
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RuleEngine> _logger;
        private const string RULE_CACHE_KEY = "TaxRules_{0}_{1}";
        private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

        public RuleEngine(AppDbContext context, IMemoryCache cache, ILogger<RuleEngine> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<TaxRule>> GetApplicableRulesAsync(string taxType, DateTime effectiveDate)
        {
            var cacheKey = string.Format(RULE_CACHE_KEY, taxType, effectiveDate.ToString("yyyy-MM-dd"));

            if (_cache.TryGetValue(cacheKey, out List<TaxRule> cachedRules))
            {
                _logger.LogDebug("Retrieved rules from cache for {TaxType} on {Date}", taxType, effectiveDate);
                return cachedRules;
            }

            var rules = await _context.TaxRules
                .Where(r => r.TaxType == taxType &&
                           r.IsActive &&
                           r.EffectiveFrom <= effectiveDate &&
                           (r.EffectiveTo == null || r.EffectiveTo >= effectiveDate))
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.MinAmount)
                .ToListAsync();

            _cache.Set(cacheKey, rules, CACHE_DURATION);
            _logger.LogInformation("Loaded {Count} rules for {TaxType} effective on {Date}", 
                rules.Count, taxType, effectiveDate);

            return rules;
        }

        public async Task<List<RuleApplication>> ApplyRulesAsync(string taxType, decimal amount, DateTime effectiveDate)
        {
            var applicableRules = await GetApplicableRulesAsync(taxType, effectiveDate);
            var ruleApplications = new List<RuleApplication>();

            if (!applicableRules.Any())
            {
                _logger.LogWarning("No applicable rules found for {TaxType} on {Date}", taxType, effectiveDate);
                return ruleApplications;
            }

            // For progressive tax (like income tax), apply brackets
            if (taxType == TaxTypes.Income)
            {
                decimal remainingAmount = amount;
                decimal previousMax = 0;

                foreach (var rule in applicableRules.OrderBy(r => r.MinAmount ?? 0))
                {
                    if (remainingAmount <= 0) break;

                    var taxableInBracket = CalculateTaxableAmountInBracket(
                        amount, 
                        rule.MinAmount ?? 0, 
                        rule.MaxAmount);

                    if (taxableInBracket > 0)
                    {
                        var calculatedAmount = (taxableInBracket * rule.Rate) + rule.FlatAmount;
                        
                        ruleApplications.Add(new RuleApplication
                        {
                            RuleName = rule.RuleName,
                            Rate = rule.Rate,
                            FlatAmount = rule.FlatAmount,
                            CalculatedAmount = calculatedAmount,
                            MinAmount = rule.MinAmount,
                            MaxAmount = rule.MaxAmount
                        });
                    }
                }
            }
            else
            {
                // For flat taxes (sales, property, corporate), apply the first matching rule
                var applicableRule = applicableRules.FirstOrDefault(r => r.AppliesToAmount(amount));
                
                if (applicableRule != null)
                {
                    var calculatedAmount = (amount * applicableRule.Rate) + applicableRule.FlatAmount;
                    
                    ruleApplications.Add(new RuleApplication
                    {
                        RuleName = applicableRule.RuleName,
                        Rate = applicableRule.Rate,
                        FlatAmount = applicableRule.FlatAmount,
                        CalculatedAmount = calculatedAmount,
                        MinAmount = applicableRule.MinAmount,
                        MaxAmount = applicableRule.MaxAmount
                    });
                }
            }

            _logger.LogDebug("Applied {Count} rules for {TaxType} on amount {Amount}", 
                ruleApplications.Count, taxType, amount);

            return ruleApplications;
        }

        private decimal CalculateTaxableAmountInBracket(decimal totalAmount, decimal bracketMin, decimal? bracketMax)
        {
            if (totalAmount <= bracketMin) return 0;

            var effectiveMax = bracketMax ?? totalAmount;
            var taxableAmount = Math.Min(totalAmount, effectiveMax) - bracketMin;
            
            return Math.Max(0, taxableAmount);
        }
    }
}