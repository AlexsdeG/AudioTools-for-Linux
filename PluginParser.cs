using System;
using System.Collections.Generic;
using System.IO;

public static class PluginParser
{
    public static List<(string Name, string Type, string Location)> ParsePluginStatus(string output)
    {
        var plugins = new List<(string Name, string Type, string Location)>();
        if (string.IsNullOrEmpty(output)) return plugins;

        var lines = output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string currentDirectory = string.Empty;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            var trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (trimmed.StartsWith("/") && !trimmed.Contains("::"))
            {
                currentDirectory = trimmed.TrimEnd('/');
                continue;
            }

            if (!line.StartsWith("  ") || !trimmed.Contains("::")) continue;

            var pluginParts = trimmed.Split(new[] { "::" }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (pluginParts.Length < 2) continue;

            var relativePluginPath = pluginParts[0].Trim();
            var metadataTokens = pluginParts[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var pluginType = metadataTokens.Length > 0 ? metadataTokens[0].Trim() : "Unknown";

            var relativeDirectory = Path.GetDirectoryName(relativePluginPath);
            var location = string.IsNullOrEmpty(relativeDirectory)
                ? currentDirectory
                : Path.Combine(currentDirectory, relativeDirectory).Replace("\\", "/");

            var pluginName = Path.GetFileNameWithoutExtension(relativePluginPath);
            if (string.IsNullOrEmpty(pluginName)) pluginName = relativePluginPath;

            // Normalize location: if still empty use dash placeholder
            if (string.IsNullOrWhiteSpace(location)) location = "-";

            plugins.Add((pluginName, pluginType, location));
        }

        return plugins;
    }
}
