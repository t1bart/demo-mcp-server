using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NineAFirstMCPServer;

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
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ExternalApiService>();

await builder.Build().RunAsync();