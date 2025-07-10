using System;
using System.ComponentModel.DataAnnotations;
using OklahomaTaxEngine.Data;

namespace OklahomaTaxEngine.Models
{
    public class TaxRule : BaseEntity
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TaxType { get; set; } // Income, Sales, Property, Corporate
        
        [Required]
        [StringLength(200)]
        public string RuleName { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? MinAmount { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? MaxAmount { get; set; }
        
        [Required]
        [Range(0, 1)]
        public decimal Rate { get; set; } // Tax rate as decimal (e.g., 0.05 for 5%)
        
        [Range(0, double.MaxValue)]
        public decimal FlatAmount { get; set; } = 0; // Fixed amount to add
        
        [Required]
        public DateTime EffectiveFrom { get; set; }
        
        public DateTime? EffectiveTo { get; set; }
        
        public int Priority { get; set; } = 0; // Higher priority rules apply first
        
        public bool IsActive { get; set; } = true;
        
        // Validation method
        public bool IsEffectiveOn(DateTime date)
        {
            return IsActive && 
                   date >= EffectiveFrom && 
                   (EffectiveTo == null || date <= EffectiveTo);
        }
        
        public bool AppliesToAmount(decimal amount)
        {
            return (MinAmount == null || amount >= MinAmount) &&
                   (MaxAmount == null || amount <= MaxAmount);
        }
    }
}