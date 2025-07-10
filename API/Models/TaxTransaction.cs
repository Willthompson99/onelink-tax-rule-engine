using System;
using System.ComponentModel.DataAnnotations;
using OklahomaTaxEngine.Data;

namespace OklahomaTaxEngine.Models
{
    public class TaxTransaction : BaseEntity
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TransactionId { get; set; }
        
        [Required]
        public int TaxPayerId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TaxType { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TaxableAmount { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public DateTime TransactionDate { get; set; }
        
        [StringLength(20)]
        public string TaxPeriod { get; set; } // e.g., "2024-Q1", "2024-03"
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled, Refunded
        
        [StringLength(50)]
        public string PaymentMethod { get; set; } // Credit Card, Bank Transfer, Check
        
        public DateTime? PaymentDate { get; set; }
        
        public string Notes { get; set; }
        
        // Navigation property
        public virtual TaxPayer TaxPayer { get; set; }
        
        // Helper methods
        public void MarkAsPaid(string paymentMethod)
        {
            Status = "Paid";
            PaymentMethod = paymentMethod;
            PaymentDate = DateTime.UtcNow;
        }
        
        public bool IsPaid => Status == "Paid";
        public bool IsPending => Status == "Pending";
    }
}