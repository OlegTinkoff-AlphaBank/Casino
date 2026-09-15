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

    public async Task<string?> GetLast(int page = 1)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await client.GetAsync($"https://www.donationalerts.com/api/v1/alerts/donations");
        var Json = await response.Content.ReadAsStringAsync();
        using var data = JsonDocument.Parse(Json);
        if (data.RootElement.TryGetProperty("data", out var dataElement))
        {
            return dataElement.GetRawText();
        }
        return Json;
    }

}