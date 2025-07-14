using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Projects;

namespace Aspire.Hosting.WebhooksTester.Tests.Tests;

public class IntegrationTest1
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private sealed record CapturedRequest(string request_payload_base64);

    [Fact]
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

        await using var app = await builder.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        await app.ResourceNotifications.WaitForResourceAsync("webhook-tester", KnownResourceStates.Running, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var apiClient = app.CreateHttpClient("api");
        var webhookClient = app.CreateHttpClient("webhook-tester", "http");

        // The webhook client base address ends with the default session token
        var sessionToken = webhookClient.BaseAddress?.Segments.Last().TrimEnd('/');

        // Ensure the session exists
        var sessionResponse = await webhookClient.GetAsync($"/api/session/{sessionToken}", cancellationToken);
        sessionResponse.EnsureSuccessStatusCode();

        var payload = new { name = "Rebecca" };
        var response = await apiClient.PostAsJsonAsync("/test", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        var requests = await webhookClient.GetFromJsonAsync<List<CapturedRequest>>($"/api/session/{sessionToken}/requests", cancellationToken);
        Assert.NotNull(requests);
        Assert.NotEmpty(requests!);

        var bodyJson = Encoding.UTF8.GetString(Convert.FromBase64String(requests![0].request_payload_base64));
        Assert.Contains("Rebecca", bodyJson);
    }
}
