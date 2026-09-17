using Microsoft.EntityFrameworkCore;

namespace serverDB;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public decimal Balance { get; set; }
    public string Role { get; set; } = "User";
    public string DonationAlertsCode { get; set; } 
    public decimal GameRate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GameRound> GameRounds { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
    public ICollection<Donation> Donations { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}