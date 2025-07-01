namespace onelink_tax_rule_engine.Api.Models;

public class ClaimType
{
    public int    ClaimTypeID { get; set; }
    public string TypeName    { get; set; } = string.Empty;

    // nav-prop
    public ICollection<Claim> Claims { get; set; } = [];
}
