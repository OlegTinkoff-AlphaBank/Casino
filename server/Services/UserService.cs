using Microsoft.EntityFrameworkCore;
using server.DTOs;
using serverDB;

namespace server.Services;

public class UserService
{
    private readonly ServerDbContext _db;

    public UserService(ServerDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task RefreshMasterUsersRateAsync()
    {
        var availableNow = await _db.Activs.Where(a => a.id == 1).Select(a => a.OnAccsAmount).FirstAsync();
        var availableLast = await _db.Activs.Where(a => a.id == 1).Select(a => a.OnAccsAmountLast).FirstAsync();
        var canuot =  await _db.Activs.Where(a => a.id == 1).Select(a => a.OutCanAmount).FirstAsync();
        
        if (canuot - availableNow <= canuot * 0.20m)
        {
            (await _db.Activs.FirstAsync(a => a.id == 1)).AllUsersRate -= 0.05m;
            await _db.SaveChangesAsync();
        }
        else if (canuot - availableLast >= canuot * 0.60m)
        {
            (await _db.Activs.FirstAsync(a => a.id == 1)).AllUsersRate += 0.1m;
        }
        await _db.SaveChangesAsync();
    }

    public async Task RefreshUsersRateAsync()
    {
        var users = await _db.Users.ToListAsync();
        
        var activeUsers = await _db.Users
            .Where(u => u.GameRounds.Any(g =>
                g.PlayedAt >= DateTime.UtcNow.AddDays(-3)))
            .ToListAsync();
        
        var inactiveUsers = await _db.Users
            .Where(u => u.GameRounds.Any(g =>
                g.PlayedAt >= DateTime.UtcNow.AddDays(-3)))
            .ToListAsync();

        int countA = (int)Math.Ceiling(activeUsers.Count * 0.30);
        int countI = (int)Math.Ceiling(inactiveUsers.Count * 0.20);

        var selectedUsersA = activeUsers
            .OrderBy(_ => Random.Shared.Next())
            .Take(countA)
            .ToList();
        
        var selectedUsersI = inactiveUsers
            .OrderBy(_ => Random.Shared.Next())
            .Take(countI)
            .ToList();
        
        foreach (var user in users)
        {
            user.GameRate = (decimal)(Random.Shared.NextDouble() * 0.3) + 0.2m;
        }
        
        foreach (var user in selectedUsersA)
        {
            user.GameRate = (decimal)(Random.Shared.NextDouble() * 0.3) + 0.6m;
        }
        
        foreach (var user in selectedUsersI)
        {
            user.GameRate = (decimal)(Random.Shared.NextDouble() * 0.3) + 0.6m;
        }
        await _db.SaveChangesAsync();
    }
    
    public async Task<decimal> AddBalanceAsync(string username, decimal amount)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username)
                   ?? throw new InvalidOperationException("Пользователь не найден");

        user.Balance += amount;

        _db.Transactions.Add(new Transaction
        {
            UserId = user.Id,
            Type = "AdminAdjustment",
            Amount = amount,
            BalanceAfter = user.Balance,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return user.Balance;
    }
    
    public async Task<Game> CreateGameAsync(CreateGameDto dto)
    {
        var game = new Game
        {
            Name = dto.Name,
            MinBet = dto.MinBet,
            MaxBet = dto.MaxBet,
            IsActive = true,
            GameRate = dto.GameRate
        };

        _db.Games.Add(game);
        await _db.SaveChangesAsync();
        return game;
    }
    
    public async Task<string?> SaveOutcomeImageAsync(int outcomeId, IFormFile file, string webRootPath)
    {
        var outcome = await _db.GameOutComes.FindAsync(outcomeId);
        if (outcome is null) return null;

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var folder = Path.Combine(webRootPath, "images", "outcomes");
        Directory.CreateDirectory(folder);

        await using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        outcome.ImageUrl = $"/images/outcomes/{fileName}";
        await _db.SaveChangesAsync();
        return outcome.ImageUrl;
    }
    
    public async Task<GameOutComes> CreateOutcomeAsync(CreateGameOutcomeDto dto)
    {
        var gameExists = await _db.Games.AnyAsync(g => g.Id == dto.GameId);
        if (!gameExists) throw new InvalidOperationException("Игра не найдена");

        var outcome = new GameOutComes
        {
            GameId = dto.GameId,
            Multiplier = dto.Multiplier,
            Weight = dto.Weight,
            IsActive = true
        };

        _db.GameOutComes.Add(outcome);
        await _db.SaveChangesAsync();
        return outcome;
    }
}