namespace serverDB;

public class Promocode
{
    public int Id { get; set; }
    public string Code { get; set; }
    public decimal Value { get; set; }
    public int? MaxUses { get; set; }
    public int MaxUsesPerUser { get; set; }
    public bool IsActive { get; set; }
    public decimal MaxCash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Transaction> Transactions { get; set; }
}