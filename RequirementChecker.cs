using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public static class RequirementChecker
{
    public static async Task<(bool IsInstalled, string Version)> CheckToolAsync(string toolName, string versionFlag = "--version")
    {
        try
        {
            var whichOut = await ProcessUtils.RunCommandWithReturnAsync($"which {toolName}");
            if (string.IsNullOrWhiteSpace(whichOut))
            {
                return (false, string.Empty);
            }

            // Try to get a version string; some tools print to stderr so capture both
            var versionOut = await ProcessUtils.RunCommandWithReturnAsync($"{toolName} {versionFlag}");
            versionOut = (versionOut ?? string.Empty).Trim();
            return (true, versionOut);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public static (bool IsOlderThan921, string Version) ParseWineVersion(string versionOutput)
    {
        if (string.IsNullOrWhiteSpace(versionOutput)) return (false, string.Empty);

        // Extract first numeric version-like token (e.g. 9.0, 9.21)
        var m = Regex.Match(versionOutput, @"\d+(?:\.\d+)+");
        if (!m.Success) return (false, versionOutput.Trim());

        var verStr = m.Value;
        try
        {
            var parsed = new Version(verStr);
            var threshold = new Version(9, 21);
            return (parsed < threshold, verStr);
        }
        catch
        {
            return (false, verStr);
        }
    }

    public static async Task<(bool IsInstalled, bool IsOlderThan921, string Version)> CheckWineAsync()
    {
        var (installed, versionOut) = await CheckToolAsync("wine");
        if (!installed) return (false, false, string.Empty);

        var (isOlder, verStr) = ParseWineVersion(versionOut);
        return (true, isOlder, verStr);
    }

    public static async Task<bool> CheckMicrosoftFontsAsync()
    {
        try
        {
            var outp = await ProcessUtils.RunCommandWithReturnAsync("fc-list | grep -i mscorefonts");
            return !string.IsNullOrWhiteSpace(outp);
        }
        catch
        {
            return false;
        }
    }
}
