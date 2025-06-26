namespace Api.Models;

public class Transaction
{
    public int Id { get; set; }
    public string ItemType { get; set; } = string.Empty; // "Digital" or "Physical"
    public decimal Amount { get; set; }
    public decimal TaxDue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
