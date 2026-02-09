using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace NineAFirstMCPServer;

[McpServerToolType]
public static class DemoTool
{
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