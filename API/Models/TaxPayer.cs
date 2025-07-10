using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OklahomaTaxEngine.Data;

namespace OklahomaTaxEngine.Models
{
    public class TaxPayer : BaseEntity
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TaxId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } // Individual, Corporate, Partnership, etc.
        
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }
        
        [StringLength(500)]
        public string Address { get; set; }
        
        [StringLength(100)]
        public string City { get; set; }
        
        [StringLength(2)]
        public string State { get; set; }
        
        [StringLength(10)]
        public string ZipCode { get; set; }
        
        public DateTime RegistrationDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public virtual ICollection<TaxTransaction> Transactions { get; set; } = new List<TaxTransaction>();
    }
}