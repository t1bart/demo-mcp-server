using System.ComponentModel;
using ModelContextProtocol.Server;

namespace NineAFirstMCPServer;

[McpServerToolType]
public class ExternalApiTool
{
    private readonly ExternalApiService _externalApiService;

    public ExternalApiTool(ExternalApiService externalApiService)
    {
        _externalApiService = externalApiService;
    }

    [McpServerTool, Description("Fetches a random joke from an external API")]
    public async Task<string> GetRandomJokeAsync()
    {
        return await _externalApiService.GetJokeAsync();
    }
}