using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public static class YabridgeService
{
    public static Task<int> RunCommandAsync(string command, IProgress<string> progress, CancellationToken ct = default)
    {
        return ProcessUtils.RunCommandAsync(command, progress, ct);
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
