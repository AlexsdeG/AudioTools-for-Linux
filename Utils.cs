using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public static class ProcessUtils
{
    public static void RunCommand(string command, Action<string>? onOutput = null)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) onOutput?.Invoke(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) onOutput?.Invoke("Error: " + e.Data); };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }

    public static async Task<int> RunCommandAsync(string command, IProgress<string>? progress, CancellationToken ct = default)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true })
        {
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data)) progress?.Report(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data)) progress?.Report("Error: " + args.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (ct.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch { } }))
            {
                await process.WaitForExitAsync(ct);
            }

            return process.ExitCode;
        }
    }

    public static async Task<string> RunCommandWithReturnAsync(string command, CancellationToken ct = default)
    {
        var output = new List<string>();

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true })
        {
            process.OutputDataReceived += (sender, args) => { if (args.Data != null) output.Add(args.Data); };
            process.ErrorDataReceived += (sender, args) => { if (args.Data != null) output.Add("Error: " + args.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (ct.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch { } }))
            {
                await process.WaitForExitAsync(ct);
            }
        }

        return string.Join("\n", output);
    }

    public static string RunCommandWithReturn(string command)
    {
        return RunCommandWithReturnAsync(command).GetAwaiter().GetResult();
    }
}
