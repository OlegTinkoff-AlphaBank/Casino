using Microsoft.EntityFrameworkCore;
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
}