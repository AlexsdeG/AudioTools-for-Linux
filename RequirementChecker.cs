using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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

    public static async Task<bool> CheckMicrosoftFontsAsync_Robust()
    {
        try
        {
            // Check common installation directories for msttcorefonts
            var paths = new[] {
                "/usr/share/fonts/truetype/msttcorefonts",
                "/usr/share/fonts/truetype/msttcorefonts/Arial.ttf",
                "/usr/share/fonts/truetype/msttcorefonts/Times_New_Roman.ttf",
                "/usr/share/fonts/truetype/msttcorefonts/Times New Roman.ttf",
                "/usr/share/fonts/truetype/msttcorefonts/Times.ttf",
                "/usr/share/fonts/truetype/msttcorefonts/arial.ttf"
            };

            foreach (var p in paths)
            {
                try { if (Directory.Exists(p) || File.Exists(p)) return true; } catch { }
            }

            // Fallback: look for well-known Microsoft font family names via fc-list
            var grep = "fc-list : family | grep -i -E \"Times New Roman|Arial|Courier New|Georgia|Trebuchet\" || true";
            var outp = await ProcessUtils.RunCommandWithReturnAsync(grep);
            if (!string.IsNullOrWhiteSpace(outp)) return true;

            // Also check whether the installer package is present (Debian/Ubuntu)
            try
            {
                var dpkgOut = await ProcessUtils.RunCommandWithReturnAsync("dpkg -l ttf-mscorefonts-installer 2>/dev/null || true");
                if (!string.IsNullOrWhiteSpace(dpkgOut) && dpkgOut.Contains("ttf-mscorefonts-installer")) return true;
            }
            catch { }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
