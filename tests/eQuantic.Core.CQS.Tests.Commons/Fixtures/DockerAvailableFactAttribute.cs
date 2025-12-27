using System.Diagnostics;
using Xunit;

namespace eQuantic.Core.CQS.Tests.Commons.Fixtures;

/// <summary>
/// Skip tests when Docker is not available.
/// Use this attribute instead of [Fact] for integration tests that require Docker.
/// Works on macOS, Linux, and Windows.
/// </summary>
public sealed class DockerAvailableFactAttribute : FactAttribute
{
    private static bool? _isDockerAvailable;

    public DockerAvailableFactAttribute()
    {
        if (!IsDockerAvailable())
        {
            Skip = "Docker is not available. These integration tests require Docker to be running.";
        }
    }

    private static bool IsDockerAvailable()
    {
        if (_isDockerAvailable.HasValue)
            return _isDockerAvailable.Value;

        try
        {
            // Use docker CLI which works on all platforms
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "info",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit(5000); // 5 second timeout
            
            _isDockerAvailable = process.ExitCode == 0;
        }
        catch
        {
            _isDockerAvailable = false;
        }

        return _isDockerAvailable.Value;
    }
}
