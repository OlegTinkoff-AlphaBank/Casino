namespace serverDB;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal MinBet { get; set; }
    public decimal MaxBet { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal GameRate { get; set; }
    
    public ICollection<GameRound> GameRounds { get; set; }
    public ICollection<GameOutComes>  GameOutComes { get; set; }
}