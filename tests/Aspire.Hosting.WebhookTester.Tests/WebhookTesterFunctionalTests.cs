using System.Net;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Xunit.Abstractions;

namespace Aspire.Hosting.WebhookTester.Tests;

public class WebhookTesterFunctionalTests(ITestOutputHelper testOutputHelper)
{
    [RequiresDocker]
    public async Task VerifyWebhookTesterResourceHealth()
    {
        using var builder = DistributedApplicationTestingBuilder.Create();
        var token = Guid.NewGuid().ToString();
        var webhook = builder.AddWebhookTester("webhook-tester", token);

        using var app = builder.Build();

        await app.StartAsync();

        var client = app.CreateHttpClient(webhook.Resource.Name, "http");
        var response = await client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [RequiresDocker]
    public async Task WithDefaultWebhookTokenInjectsEnvironment()
    {
        using var builder = DistributedApplicationTestingBuilder.Create();
        var token = Guid.NewGuid().ToString();
        var webhook = builder.AddWebhookTester("webhook", token);
        var consumer = builder.AddContainer("consumer", "docker.io/library/nginx:latest")
                              .WithDefaultWebhookToken(webhook);

        using var app = builder.Build();
        await app.StartAsync();

        var env = await consumer.Resource.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
        Assert.Equal(token, env["DEFAULT_SESSION_TOKEN"]);
    }

    [RequiresDocker]
    public async Task WithFsStorageDirSetsEnvironmentVariable()
    {
        using var builder = DistributedApplicationTestingBuilder.Create();
        var dir = "/tmp";
        var webhook = builder.AddWebhookTester("webhook")
                              .WithFsStorageDir(dir);

        using var app = builder.Build();
        await app.StartAsync();

        var env = await webhook.Resource.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
        Assert.Equal(dir, env["FS_STORAGE_DIR"]);
    }
}
