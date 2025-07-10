namespace OklahomaTaxEngine.Models
{
    public static class TaxTypes
    {
        public const string Income = "Income";
        public const string Sales = "Sales";
        public const string Property = "Property";
        public const string Corporate = "Corporate";
        
        public static readonly string[] All = { Income, Sales, Property, Corporate };
        
        public static bool IsValid(string taxType)
        {
            return All.Contains(taxType, StringComparer.OrdinalIgnoreCase);
        }
    }
public class TaxCalculationRequest
    {
        [Required]
        public string TaxPayerId { get; set; }
        
        [Required]
        public string TaxType { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TaxableAmount { get; set; }
        
        public DateTime? CalculationDate { get; set; }
        
        public string TaxPeriod { get; set; }
    }
    
    public class TaxCalculationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<RuleApplication> AppliedRules { get; set; } = new();
    }
    
    public class RuleApplication
    {
        public string RuleName { get; set; }
        public decimal Rate { get; set; }
        public decimal FlatAmount { get; set; }
        public decimal CalculatedAmount { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}