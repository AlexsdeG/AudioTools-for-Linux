using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public static class YabridgeService
{
    public static Task<int> RunCommandAsync(string command, IProgress<string>? progress, CancellationToken ct = default)
    {
        // Wrap progress so that stderr lines can be additionally logged
        IProgress<string>? wrapped = null;
        if (progress != null)
        {
            wrapped = new Progress<string>(line =>
            {
                try
                {
                    if (line != null && line.StartsWith("Error:"))
                    {
                        Logger.LogStderrLine(line);
                    }
                }
                catch { }
                if (line != null) progress?.Report(line);
            });
        }

        return ProcessUtils.RunCommandAsync(command, wrapped ?? progress, ct);
    }

    public static Task<string> RunCommandWithReturnAsync(string command, CancellationToken ct = default)
    {
        return ProcessUtils.RunCommandWithReturnAsync(command, ct);
    }

    public static string RunCommandWithReturn(string command)
    {
        return ProcessUtils.RunCommandWithReturn(command);
    }
}
