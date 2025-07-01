using System.Collections.Generic;   // ← ICollection<>
namespace onelink_tax_rule_engine.Api.Models;

public class ClaimType
{
    public int    ClaimTypeID { get; set; }
    public string TypeName    { get; set; } = string.Empty;

    // one-to-many back-reference
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
