using System;                       // ← DateTime
namespace onelink_tax_rule_engine.Api.Models;

public class Claim                  // singular class name
{
    public int      ClaimID      { get; set; }
    public string   ClaimantName { get; set; } = string.Empty;
    public decimal  ClaimAmount  { get; set; }
    public DateTime ClaimDate    { get; set; }
    public string   Status       { get; set; } = string.Empty;

    // FK + navigation -------------------------------
    public int       ClaimTypeID { get; set; }
    public ClaimType? ClaimType  { get; set; }
}
