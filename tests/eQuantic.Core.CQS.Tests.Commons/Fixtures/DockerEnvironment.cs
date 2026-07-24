using System.Diagnostics;

namespace eQuantic.Core.CQS.Tests.Commons.Fixtures;

/// <summary>
/// Whether a usable Docker daemon is present. Container fixtures ask this before starting anything:
/// an xUnit collection fixture initializes even when every test in the collection is skipped, so a
/// fixture that starts a container unconditionally hangs the whole run on an agent without Docker
/// (a Windows runner, for instance, cannot run the Linux images these fixtures use).
/// </summary>
public static class DockerEnvironment
{
    private static bool? _isAvailable;

    /// <summary>True when `docker info` succeeds, so containers can actually be started.</summary>
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
                    Arguments = "info",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            if (!process.WaitForExit(5000))
            {
                process.Kill(entireProcessTree: true);
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
