namespace serverDB;

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public int? RelatedGameRoundId { get; set; }
    public int? RelatedDonationId { get; set; }
    public int? RelatedPromocodeId { get; set; }
    public Promocode Promocode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}