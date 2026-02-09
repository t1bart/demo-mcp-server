using System.Text.Json;

namespace NineAFirstMCPServer;

public class ExternalApiService
{
    private readonly IHttpClientFactory _httpClient;

    public ExternalApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory;
    }

    public async Task<string> GetJokeAsync()
    {
        try
        {
            var client = _httpClient.CreateClient();
            var response = await client.GetAsync("https://official-joke-api.appspot.com/random_joke");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var joke = JsonSerializer.Deserialize<Joke>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return joke != null ? $"{joke.Setup} - {joke.Punchline}" : "No joke found.";
        }
        catch (HttpRequestException ex)
        {
            return $"HTTP error occurred: {ex.Message}";
        }
        catch (JsonException ex)
        {
            return $"Failed to parse response: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"{nameof(ExternalApiService)} failed: {ex.Message}";
        }
    }

    private class Joke
    {
        public string Setup { get; set; }
        public string Punchline { get; set; }
    }
}
    