using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

public class SearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _googleApiKey;
    private readonly string _tmdbApiKey;
    private readonly string _giantBombApiKey;
    private readonly string _lastFmApiKey;

    public SearchService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _googleApiKey = configuration["Google:ApiKey"]
            ?? throw new ArgumentNullException("Google API key is missing in configuration");

        _tmdbApiKey = configuration["TMDB:ApiKey"]
            ?? throw new ArgumentNullException("TMDB API key is missing in configuration");

        _giantBombApiKey = configuration["GiantBomb:ApiKey"]
            ?? throw new ArgumentNullException("GiantBomb API key is missing in configuration");

        _lastFmApiKey = configuration["LastFM:ApiKey"]
            ?? throw new ArgumentNullException("LastFM API key is missing in configuration");
    }


    public async Task<string?> SearchBooksAsync(string query)
    {
        string url = $"https://www.googleapis.com/books/v1/volumes?q={query}&key={_googleApiKey}";
        try
        {
            var res = await _httpClient.GetStringAsync(url);
            return res;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error fetchign book datas: {e.Message}");
            return null;
        }
    }

    public async Task<string?> SearchGamesAsync(string query)
    {
        string url = $"https://www.giantbomb.com/api/search/?api_key={_giantBombApiKey}&query={query}&format=json";
        var req = new HttpRequestMessage(HttpMethod.Get, url);

        req.Headers.Add("User-Agent", "MemoryDrawerApp/1.0");
        var res = await _httpClient.SendAsync(req);

        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        return await res.Content.ReadAsStringAsync();
    }

    public async Task<string?> SearchMoviesAsync(string query)
    {
        string url = $"https://api.themoviedb.org/3/search/movie?query={query}";
        var req = new HttpRequestMessage(HttpMethod.Get, url); // send a get request 

        // attach headers
        req.Headers.Add("Authorization", $"Bearer {_tmdbApiKey}");
        var res = await _httpClient.SendAsync(req);

        Console.WriteLine($"TMDB Response Status: {res.StatusCode}");


        if (!res.IsSuccessStatusCode)
        {
            Console.WriteLine("TMDB API Error: " + await res.Content.ReadAsStringAsync());
            return null;
        }

        string responseData = await res.Content.ReadAsStringAsync();
        Console.WriteLine($"TMDB API Response: {responseData}");  // Log raw JSON response

        return responseData;
    }

    public async Task<string?> SearchMusicAsync(string query)
    {
        string url = $"http://ws.audioscrobbler.com/2.0/?method=album.search&album={query}&api_key={_lastFmApiKey}&format=json";

        try
        {
            var res = await _httpClient.GetStringAsync(url);
            return res;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching music data: {ex.Message}");
            return null;
        }
    }
}