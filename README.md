# Build your own MCP server

For this workshop we are using .Net 9, Visual Studio Code and GitHub Copilot. Please make sure all is installed, configured and licensed. 

Respources:
- [The official C# SDK for the MCP](https://github.com/modelcontextprotocol/csharp-sdk)

## Local
You can Skip steps 2 through 6 if you install the MCP Server template like so:
```shell
dotnet new install Microsoft.McpServer.ProjectTemplates
dotnet new mcpserver
```

1. Open a terminal (in VS code <kbd>CTRL</kbd>+<kbd>Shift</kbd>+<kbd>`</kbd> and type `code -r {path}`) and go to the location where your first MCP server project wants to live. 
```shell
cd C:\Repos\9A\MCP
```

2. Create new console app
```shell
dotnet new console -n NineAFirstMCPServer
```

4. Go to project folder
```Shell
cd .\NineAFirstMCPServer\
```

5. Add NuGet packages
```shell
dotnet add package ModelContextProtocol --prerelease
dotnet add package Microsoft.Extensions.Hosting
```

6. Open `Program.cs` in VS code and replace existing code with the following:
```c#
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    // stdout is reserved for MCP communication, divert logs to stderr
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add MCP service
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();  // Scan assembly for classes marked with [McpServerToolType] attribute 
    
// Add additional services

   
await builder.Build().RunAsync();
```

7. Add a tool class to your project:
```Shell
dotnet new class -n DemoTool
```

8. Open the newly created class file `DemoTool.cs` and add replace the code with the following:
```c#
using System.ComponentModel;
using ModelContextProtocol.Server;

namespace NineAFirstMCPServer;

[McpServerToolType]
public static class DemoTool
{

}
```

9. Now add the some tools to this class:
```c#
[McpServerToolType]
public static class DemoTool
{
	[McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";
    
	[McpServerTool, Description("Tell MCP server name to the client.")]
    public static string MyName() => $"My name is '{nameof(NineAFirstMCPServer)} von NineAltitudes'.";
    
    [McpServerTool, Description("Converts the message to camelCase.")]
    public static string CamelCase(string message)
    {
        var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return string.Empty;
        
        var result = words[0].ToLower();
        for (int i = 1; i < words.Length; i++)
        {
            if (words[i].Length > 0)
                result += char.ToUpper(words[i][0]) + words[i][1..].ToLower();
        }
        return result;
    }
}
```

10. Create `.vscode` folder in root of the project, if not already exists.
```shell
mkdir .vscode
```
    
11. In this folder create a file named `mcp.json`. Open it and copy paste the following json:
```json
{
    "inputs": [],
    "servers": {
        "NineAFirstMCP": {
            "type": "stdio",
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "{path to .csproj file}" <- change
            ]
        }
    }
}
```

12. Start MCP server: on save it could start the server automatically. Else start the server by clicking `Start` onder "servers" in `mcp.json` or press <kbd>CTRL</kbd>+<kbd>Shft</kbd>+<kbd>P</kbd> and type in "MCP" and select "MCP: List Servers" and choice *NineAFirstMCP* followed by *Start Server*.
13. Prompt something that could trigger the new MCP server in your VS code Github Copilot, e.g. "Camelcase the following message:'Some random message'".

Enjoy.


### Connect to existing API
To get data or make our MCP server do things we need to connect to external sources. This can be an existing API, a database or just a fileshare somewhere. Lets implement a external API to our MCP Server.

1. Create a new service class for creating the external connection:
```shell
dotnet new class -n ExternalApiService
```

2. Add required NuGet packages
```shell
dotnet add package Microsoft.Extensions.Http
```

3. Update `Program.cs` to Dependency Inject the **HttpClient**;
```c#
// Add additional services
builder.Services.AddHttpClient();
```

3. Past following code to add a Http client for communication with external API:
```c#
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

```


4. Update `Program.cs` to add the new **ExternalApiService** as a singelton
```c#
using NineAFirstMCPServer;
...

// Add additional services
...
builder.Services.AddSingleton<ExternalApiService>();
```

5. Create a new tool class:
```shell
dotnet new class -n ExternalApiTool
```

6. Add  MCP attribute `[McpServerToolType]`, add de external API service `externalApiService` and define some tools that interact with external API:
```c#
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
```

### Talk back to Agent
Tools can have the `McpServer` representing the server injected via a parameter to the method, and can use that for interaction with the connected client. Basicly you can ask the client to perform an LLM task like summerize or translate message.

You can also pre define prompts using the `[McpServerPrompt]` attribute. 

```c#
[McpServerPromptType]
public static class MyPrompts
{
    [McpServerPrompt, Description("Creates a prompt to summarize the provided message.")]
    public static ChatMessage Summarize([Description("The content to summarize")] string content) =>
        new(ChatRole.User, $"Please summarize this content into a single sentence: {content}");
}
```

### Configure hpw to handle client request
...

## Container
...