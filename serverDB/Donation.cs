namespace serverDB;

public class Donation
{
    public int Id { get; set; }
    public string ExternalId { get; set; }
    public int? UserId { get; set; }
    public User User { get; set; }
    public decimal Amount { get; set; }
    public string RawComment { get; set; }
    public string Status { get; set; } = "Unmatched";
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
