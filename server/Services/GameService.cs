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

    public async Task PlayRoundByGameIdAsync(PlayDTO dto, int userId)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        Game? game = await _db.Games.FirstOrDefaultAsync(g => g.Id == dto.GameId);
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
        //...
    }
}