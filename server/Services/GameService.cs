using Microsoft.EntityFrameworkCore;
using serverDB;
using server.DTOs;

namespace server.Services;

public interface IGameEngine
{
    (bool isWin, decimal payout, object resultData) Play(decimal betAmount, Game gameConfig);
}

public class GameService
{
    private readonly ServerDbContext _db;

    public GameService(ServerDbContext db)
    {
        _db = db;
    }

    public async Task<List<Game>> GetActiveGamesListAsync()
    {
        return await _db.Games.Where(g => g.IsActive).ToListAsync();
    }

    public async Task<Game?> GetGameById(int id)
    {
        return await _db.Games.FindAsync(id);
    }

    public async Task<GameOutComes> PlayRoundByGameIdAsync(PlayDTO dto)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        Game? game = await _db.Games.FirstOrDefaultAsync(g => g.Id == dto.GameId);
        Activs? activs = await _db.Activs.FirstOrDefaultAsync();
        List<GameOutComes> goc = await _db.GameOutComes.Where(go => go.GameId == dto.GameId && go.IsActive).ToListAsync();
        
        if (user == null) 
            throw new Exception("User not found");
        if (user.Balance < dto.BetAmount)
            throw new Exception("balance is too small");
        
        if (game == null || !game.IsActive)
            throw new Exception("Game not found");
        if (dto.BetAmount > game.MaxBet || dto.BetAmount < game.MinBet)
            throw new Exception("bet is not valid");
        
        user.Balance -= dto.BetAmount;
        decimal rate = (game.GameRate + user.GameRate + activs.AllUsersRate + activs.AllGamesRate) / 4m;
        decimal power = Math.Max(0.1m, 5.0m * (1.0m - rate));

        var modOutComes = goc.Select(g => new
        {
            GameOutComes = g,
            ModWeight = (decimal)Math.Pow((double)g.Weight, (double)power)
        });
        
        decimal totalweight = modOutComes.Sum(m => m.ModWeight);
        decimal roll = (decimal)Random.Shared.NextDouble() * totalweight;
        decimal cSum = 0m;

        foreach (var outcome in modOutComes)
        {
            cSum += outcome.ModWeight;
            if (roll <= cSum)
            {
                user.Balance += dto.BetAmount * outcome.GameOutComes.Multiplier;
                await _db.SaveChangesAsync();
                return outcome.GameOutComes;
            }
        }
        user.Balance += dto.BetAmount;
        await _db.SaveChangesAsync();
        throw new Exception("internal game error");
    }
}