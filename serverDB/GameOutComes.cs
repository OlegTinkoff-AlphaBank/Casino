namespace serverDB;

public class GameOutComes
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; }
    public decimal Multiplier { get; set; }
    public decimal Weight { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
}