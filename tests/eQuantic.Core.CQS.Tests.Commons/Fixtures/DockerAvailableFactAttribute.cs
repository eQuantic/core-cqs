using Xunit;

namespace eQuantic.Core.CQS.Tests.Commons.Fixtures;

/// <summary>
/// Skip tests when Docker is not available.
/// Use this attribute instead of [Fact] for integration tests that require Docker.
/// Works on macOS, Linux, and Windows.
/// </summary>
public sealed class DockerAvailableFactAttribute : FactAttribute
{
    public DockerAvailableFactAttribute()
    {
        if (!DockerEnvironment.IsAvailable)
        {
            Skip = "Docker is not available. These integration tests require Docker to be running.";
        }
    }
}
