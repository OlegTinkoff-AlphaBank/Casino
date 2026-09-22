using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    
    public record GameOutcomeDto(int Id, int GameId, decimal M, decimal W, string? ImageUrl);

    public async Task<List<GameOutcomeDto>> GetOutcomesAsync(int gameId)
    {
        return await _db.GameOutComes
            .Where(o => o.GameId == gameId && o.IsActive)
            .Select(o => new GameOutcomeDto(o.Id, o.GameId, o.Multiplier, o.Weight, o.ImageUrl))
            .ToListAsync();
    }
    
    public async Task<List<Game>> GetActiveGamesListAsync()
    {
        return await _db.Games.Where(g => g.IsActive).ToListAsync();
    }

    public async Task<Game?> GetGameById(int id)
    {
        return await _db.Games.FindAsync(id);
    }

    public async Task<PlayResultDto> PlayRoundByGameIdAsync(PlayDTO dto, int userId)
    {
        Console.WriteLine("starting round");
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
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

        Console.WriteLine("Checking ready");
        
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

        Console.WriteLine("before rate");

        foreach (var outcome in modOutComes)
        {
            cSum += outcome.ModWeight;
            if (roll <= cSum)
            {
                user.Balance += dto.BetAmount * outcome.GameOutComes.Multiplier;
                var jsondata = new 
                {
                    GameOutComes = new 
                    {
                        Id = outcome.GameOutComes.Id,
                        GameId = outcome.GameOutComes.GameId,
                        Multiplier = outcome.GameOutComes.Multiplier,
                        Weight = outcome.GameOutComes.Weight,
                        IsActive = outcome.GameOutComes.IsActive,
                        GameName = outcome.GameOutComes.Game?.Name 
                    },
                    ModWeight = new 
                    { 
                        weight = outcome.ModWeight
                    }
                };
                string jsonOut = JsonSerializer.Serialize(jsondata,
                    new JsonSerializerOptions
                        { Encoder = JavaScriptEncoder.Default, WriteIndented = true });
                GameRound gameRound = new GameRound
                {
                    GameId = dto.GameId,
                    UserId = user.Id,
                    PlayedAt = DateTime.UtcNow,
                    Status = "finished",
                    ResultData = jsonOut,
                    Payout = dto.BetAmount * outcome.GameOutComes.Multiplier,
                    BetAmount = dto.BetAmount
                };
                _db.GameRounds.Add(gameRound);
                await _db.SaveChangesAsync();
                _db.Transactions.Add(new Transaction
                {
                    UserId = user.Id,
                    Type = "GameRound",
                    Amount = (dto.BetAmount * outcome.GameOutComes.Multiplier)-dto.BetAmount,
                    BalanceAfter = user.Balance,
                    RelatedGameRoundId = gameRound.Id,
                });
                await _db.SaveChangesAsync();
                return new PlayResultDto(dto.BetAmount * outcome.GameOutComes.Multiplier >= dto.BetAmount, dto.BetAmount * outcome.GameOutComes.Multiplier, user.Balance, outcome.GameOutComes.Id);
            }
        }
        user.Balance += dto.BetAmount;
        await _db.SaveChangesAsync();
        throw new Exception("internal game error");
    }
}