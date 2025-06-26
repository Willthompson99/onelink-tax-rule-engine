namespace Api.Models;

public class RuleAudit
{
    public int Id { get; set; }
    public int RuleId { get; set; }
    public string ChangedBy { get; set; } = "system";
    public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
    public string DiffJson { get; set; } = string.Empty;
}
