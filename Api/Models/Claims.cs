namespace Api.Models
{
    public class Claim
    {
        public int ClaimID { get; set; }
        public string ClaimantName { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? ClaimTypeID { get; set; }  // optional for now
    }
}
