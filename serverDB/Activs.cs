namespace serverDB;

public class Activs
{
    public int id { get; set; }
    
    public decimal InAmount { get; set; }
    public decimal RealInAmount { get; set; }
    public decimal OnAccsAmount { get; set; }
    public decimal OnAccsAmountLast { get; set; }
    public decimal OutCanAmount { get; set; }
    public decimal OnSafeAmount { get; set; }
    public decimal AllUsersRate { get; set; }
    public decimal AllGamesRate { get; set; }
    public string DART { get; set; }
    public int DAEI { get; set; }
}