using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace server.Services;
using serverDB;

public class TokenConfig
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public int ExpiresIn { get; set; }
}

public class DonationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly String _accessToken;
    private readonly IConfiguration _config;
    private readonly ServerDbContext _db;
    public DonationService(ServerDbContext db, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _config = config;
        _httpClientFactory = httpClientFactory;
        _accessToken = _config["DonationAlerts:AccessToken"];
    }
    
    public async Task<TokenConfig> RefreshTokensAsync(string refreshToken)
    {
        var client = _httpClientFactory.CreateClient();
        var resp = await client.PostAsync("https://www.donationalerts.com/oauth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _config["DonationAlerts:ClientId"]!,
                ["client_secret"] = _config["DonationAlerts:ClientSecret"]!,
                ["refresh_token"] = refreshToken
            }));

        var json = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Refresh failed {resp.StatusCode}: {json}");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new TokenConfig
        {
            AccessToken = root.GetProperty("access_token").GetString()!,
            RefreshToken = root.GetProperty("refresh_token").GetString()!,
            ExpiresIn = root.GetProperty("expires_in").GetInt32()
        };
    }
    
    public async Task<List<Donation>> GetLastDonationsAsync(int page = 1)
    {
        Activs? activs = await _db.Activs.FirstOrDefaultAsync();
        if (activs == null) 
            throw new Exception("No activations found");
        
        var token = await RefreshTokensAsync(activs.DART);
        activs.DART = token.RefreshToken;
        activs.DAEI = token.ExpiresIn;
        await _db.SaveChangesAsync();

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var resp = await client.GetAsync($"https://www.donationalerts.com/api/v1/alerts/donations?page={page}");
        var json = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"DonationAlerts API error {resp.StatusCode}: {json}");

        using var doc = JsonDocument.Parse(json);
        var result = new List<Donation>();

        foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
        {
            result.Add(new Donation
            {
                Amount = item.GetProperty("amount").GetDecimal(),
                RawComment = item.TryGetProperty("message", out var msg) ? msg.GetString() : null
            });
        }

        return result;
    }

    public async Task<string> PostDonationAsync(int userid, decimal amount)
    {
        int code;
        bool exists;
        do
        {
            code = Random.Shared.Next(10000, 100000);
            exists = await _db.Donations.AnyAsync(d => d.RawComment == code.ToString());

        } while (exists);
        
        _db.Donations.Add(new Donation
        {
            UserId = userid,
            Amount = amount,
            RawComment = code.ToString()
        });
        await _db.SaveChangesAsync();
        return code.ToString();
    }
    
    public async Task<string> ChackDonationAsync(int userid)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userid);
        if (user == null)
            throw new Exception("User not found");
        List<Donation> donation = await _db.Donations.Where(d => d.UserId == userid && d.Status == false).ToListAsync();
        List<Donation> donations = await GetLastDonationsAsync();

        foreach (var item in donations)
        {
            foreach (var obj in donation)
            {
                if (obj.RawComment == item.RawComment)
                {
                    user.Balance += item.Amount * 0.88m;
                    obj.Status = true;
                    obj.Amount = item.Amount * 0.88m;
                    _db.Transactions.Add(new Transaction
                    {
                        UserId = user.Id,
                        Amount = item.Amount * 0.88m,
                        BalanceAfter = user.Balance,
                        Type = "Donation",
                        RelatedDonationId = obj.Id
                    });
                }
            }
        }
        await _db.SaveChangesAsync();
        return user.Balance.ToString();
    }

}