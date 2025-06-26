namespace Api.Models;

public class TaxRule
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CriteriaJson { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
}
