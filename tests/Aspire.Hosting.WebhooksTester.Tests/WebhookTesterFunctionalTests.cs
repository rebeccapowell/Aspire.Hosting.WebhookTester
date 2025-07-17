using System.Net;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Xunit.Abstractions;

namespace Aspire.Hosting.WebhooksTester.Tests;

public class WebhookTesterFunctionalTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    [RequiresDocker]
    public async Task VerifyWebhookTesterResourceHealth()
    {
        using var builder = TestDistributedApplicationBuilder.CreateWithTestContainerRegistry(testOutputHelper);
        var token = Guid.NewGuid().ToString();
        var webhook = builder.AddWebhookTester("webhook-tester", token);

        using var app = builder.Build();

        await app.StartAsync();

        var client = app.CreateHttpClient(webhook.Resource.Name, "http");
        var response = await client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    [RequiresDocker]
    public async Task WithDefaultWebhookTokenInjectsEnvironment()
    {
        using var builder = TestDistributedApplicationBuilder.CreateWithTestContainerRegistry(testOutputHelper);
        var token = Guid.NewGuid().ToString();
        var webhook = builder.AddWebhookTester("webhook", token);
        var consumer = builder.AddContainer("consumer", "docker.io/library/nginx:latest")
                              .WithDefaultWebhookToken(webhook);

        using var app = builder.Build();
        await app.StartAsync();

        var env = await consumer.Resource.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
        Assert.Equal(token, env["DEFAULT_SESSION_TOKEN"]);
    }

    [Fact]
    [RequiresDocker]
    public async Task WithFsStorageDirSetsEnvironmentVariable()
    {
        using var builder = TestDistributedApplicationBuilder.CreateWithTestContainerRegistry(testOutputHelper);
        var dir = "/tmp";
        var webhook = builder.AddWebhookTester("webhook")
                              .WithFsStorageDir(dir);

        using var app = builder.Build();
        await app.StartAsync();

        var env = await webhook.Resource.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
        Assert.Equal(dir, env["FS_STORAGE_DIR"]);
    }
}
