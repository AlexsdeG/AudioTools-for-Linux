using System;
using System.IO;

public static class Logger
{
    private static readonly object _lock = new object();
    private static string logPath;

    public static void Init()
    {
        try
        {
            var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "AudioTools");
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
            logPath = Path.Combine(configDir, "AudioTools.log");
        }
        catch
        {
            // ignore
        }
    }

    private static void Write(string level, string msg)
    {
        try
        {
            lock (_lock)
            {
                var line = $"{DateTime.UtcNow:O} [{level}] {msg}\n";
                if (!string.IsNullOrEmpty(logPath)) File.AppendAllText(logPath, line);
            }
        }
        catch
        {
            // swallow logging exceptions
        }
    }

    public static void LogInfo(string msg) => Write("INF", msg);
    public static void LogError(string msg) => Write("ERR", msg);

    public static void LogCommand(string command, string action)
    {
        var env = $"PATH={Environment.GetEnvironmentVariable("PATH")}, DISPLAY={Environment.GetEnvironmentVariable("DISPLAY")}, DBUS_SESSION_BUS_ADDRESS={Environment.GetEnvironmentVariable("DBUS_SESSION_BUS_ADDRESS")}";
        Write("CMD", $"Action={action} Command={command} Env={env}");
    }

    public static void LogStderrLine(string line)
    {
        Write("STDERR", line);
    }
}
