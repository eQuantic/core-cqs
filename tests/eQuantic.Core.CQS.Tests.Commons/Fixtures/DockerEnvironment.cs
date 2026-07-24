using System.Diagnostics;

namespace eQuantic.Core.CQS.Tests.Commons.Fixtures;

/// <summary>
/// Whether Docker can run the Linux images these fixtures use. Container fixtures ask this before
/// starting anything: an xUnit collection fixture initializes even when every test in the collection
/// is skipped, so a fixture that starts a container unconditionally hangs the whole run on an agent
/// that cannot serve it.
/// <para>
/// Presence of a daemon is not enough — a Windows agent reports a healthy Docker that only runs
/// Windows containers, and asking it for a Linux image hangs rather than failing. So the probe reads
/// the daemon's OS type and accepts it only when it is Linux.
/// </para>
/// </summary>
public static class DockerEnvironment
{
    private static bool? _isAvailable;

    /// <summary>True when a Linux-container Docker daemon answers, so these fixtures can start.</summary>
    public static bool IsAvailable => _isAvailable ??= Probe();

    private static bool Probe()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "info --format {{.OSType}}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            var osType = process.StandardOutput.ReadToEnd().Trim();
            if (!process.WaitForExit(5000))
            {
                process.Kill(entireProcessTree: true);
                return false;
            }

            return process.ExitCode == 0
                && osType.Equals("linux", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
