using System.Net.Http.Json;
using System.Text;
using Aspire.Hosting.WebhookTester.Tests;
using Microsoft.Extensions.Logging;
using Projects;

namespace Aspire.Hosting.WebhooksTester.Tests;

public class E2EIntegrationTest
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private sealed record CapturedRequest(string request_payload_base64);

    [Fact]
    [RequiresDocker]
    public async Task ApiRequestIsLoggedInWebhookTester()
    {
        var cancellationToken = TestContext.Current?.CancellationToken ?? default;

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<AppHost>(cancellationToken);

        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            logging.AddFilter(builder.Environment.ApplicationName, Microsoft.Extensions.Logging.LogLevel.Debug);
            logging.AddFilter("Aspire.", Microsoft.Extensions.Logging.LogLevel.Debug);
        });

        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        
        var webhook = builder.Resources
            .OfType<ContainerResource>()
            .First(r => r.Name == "webhook-tester");

        var env = await webhook.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
        var token = env["DEFAULT_SESSION_TOKEN"];

        await using var app = await builder.BuildAsync(cancellationToken);
        await app.StartAsync(cancellationToken);

        var apiClient = app.CreateHttpClient("api");
        var webhookClient = app.CreateHttpClient("webhook-tester", "http");

        var payload = new { name = "Rebecca" };
        var response = await apiClient.PostAsJsonAsync("/test", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        //await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

        // Query session from webhook tester using resolved token
        var sessionResponse = await webhookClient.GetAsync($"/api/session/{token}", cancellationToken);
        sessionResponse.EnsureSuccessStatusCode();

        var requests = await webhookClient.GetFromJsonAsync<List<CapturedRequest>>(
            $"/api/session/{token}/requests",
            cancellationToken);

        Assert.NotNull(requests);
        Assert.NotEmpty(requests);

        var bodyJson = Encoding.UTF8.GetString(Convert.FromBase64String(requests[0].request_payload_base64));
        Assert.Contains("Rebecca", bodyJson);
    }
}
