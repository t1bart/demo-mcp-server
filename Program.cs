using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    // stdout is reserved for MCP communication, divert logs to stderr
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// 
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();  // Scan assembly for classes marked with [McpServerToolType] attribute 
    
await builder.Build().RunAsync();