using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

public class AppSettings
{
    public int WindowWidth { get; set; } = 0;
    public int WindowHeight { get; set; } = 0;
    public int WindowX { get; set; } = 0;
    public int WindowY { get; set; } = 0;
    public string LastYabridgeAction { get; set; } = "";
    public List<string> PluginPaths { get; set; } = new List<string>();
}

public static class SettingsManager
{
    private static readonly string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "AudioTools");
    private static readonly string settingsPath = Path.Combine(configDir, "settings.json");

    public static AppSettings Instance { get; private set; } = new AppSettings();

    public static void Load()
    {
        try
        {
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Instance = JsonSerializer.Deserialize<AppSettings>(json, opts) ?? new AppSettings();
            }
            else
            {
                Instance = new AppSettings();
            }
        }
        catch
        {
            Instance = new AppSettings();
        }
    }

    public static void Save()
    {
        try
        {
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(Instance, opts);
            File.WriteAllText(settingsPath, json);
        }
        catch
        {
            // Best-effort save; failures are non-fatal
        }
    }
}
