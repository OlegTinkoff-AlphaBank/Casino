namespace serverDB;

public class GameRound
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public decimal BetAmount { get; set; }
    public string ResultData { get; set; } 
    public decimal Payout { get; set; }
    public string Status { get; set; } = "Pending";
    public Guid IdempotencyKey { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}