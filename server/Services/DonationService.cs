using System.Net.Http.Headers;
using System.Text.Json;

namespace server.Services;
using serverDB;

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
    
    private async Task<string> GetAccessTokenAsync()
    {
        var accessToken = _config["DonationAlerts:AccessToken"];
        var expiresAt = _config["DonationAlerts:ExpiresAt"];

        var client = _httpClientFactory.CreateClient();
        var resp = await client.PostAsync("https://www.donationalerts.com/oauth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _config["DonationAlerts:ClientId"]!,
                ["client_secret"] = _config["DonationAlerts:ClientSecret"]!,
                ["refresh_token"] = _config["DonationAlerts:RefreshToken"]!
            }));
        
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        accessToken = json.GetProperty("access_token").GetString();

        return accessToken!;
    }
    
    public async Task<List<Donation>> GetLastDonationsAsync(int page = 1)
    {
        var token = await GetAccessTokenAsync();

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

}