using Xunit;

namespace Aspire.Hosting.WebhookTester.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresDockerAttribute : FactAttribute
{
    public RequiresDockerAttribute(string? reason = null)
    {
        Skip = reason ?? "Docker is required to run this test.";
    }
}
