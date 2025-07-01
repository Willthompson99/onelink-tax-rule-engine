namespace onelink_tax_rule_engine.Api.Models;

public class Claim
{
    // PK
    public int    ClaimID      { get; set; }

    // data
    public string ClaimantName { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }
    public DateTime ClaimDate  { get; set; }
    public string  Status      { get; set; } = string.Empty;

    // FK / nav-prop
    public int       ClaimTypeID { get; set; }
    public ClaimType? ClaimType  { get; set; }
}
