using System.Diagnostics;

namespace CodeIsland.Windows.Services;

/// <summary>
/// Process execution helper.
/// Corresponds to ProcessExecutor.swift.
/// </summary>
public sealed class ProcessExecutor
{
    /// <summary>
    /// Execute a command and return the output.
    /// </summary>
    public async Task<(string stdout, string stderr, int exitCode)> ExecuteAsync(
        string command,
        string? arguments = null,
        string? workingDirectory = null,
        TimeSpan? timeout = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments ?? string.Empty,
            WorkingDirectory = workingDirectory ?? string.Empty,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };

        try
        {
            process.Start();

            using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(30));

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cts.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(cts.Token);

            await process.WaitForExitAsync(cts.Token);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            return (stdout, stderr, process.ExitCode);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(); } catch { }
            return (string.Empty, "Process timed out", -1);
        }
    }

    /// <summary>
    /// Execute a command in the background (fire and forget).
    /// </summary>
    public void ExecuteBackground(string command, string? arguments = null, string? workingDirectory = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments ?? string.Empty,
            WorkingDirectory = workingDirectory ?? string.Empty,
            UseShellExecute = true,
            CreateNoWindow = true
        };

        try
        {
            Process.Start(psi);
        }
        catch
        {
            // Best-effort background execution
        }
    }

    /// <summary>
    /// Check if a process with the given name is running.
    /// </summary>
    public bool IsProcessRunning(string processName)
    {
        try
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the list of running AI coding tool processes.
    /// </summary>
    public List<string> GetRunningToolProcesses()
    {
        var toolProcessNames = new[] { "claude", "codex", "gemini", "hermes", "qwen", "opencode", "copilot" };
        var running = new List<string>();

        foreach (var name in toolProcessNames)
        {
            if (IsProcessRunning(name))
                running.Add(name);
        }

        return running;
    }
}
