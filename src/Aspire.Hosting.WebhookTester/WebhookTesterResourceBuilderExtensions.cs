namespace Aspire.Hosting.WebhookTester;

public static class WebhookTesterResourceBuilderExtensions
{
    private const int DefaultContainerPort = 8080;

    public static IResourceBuilder<WebhookTesterResource> AddWebhookTester(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null)
    {
        var resource = new WebhookTesterResource(name);

        var container = builder.AddResource(resource)
            .WithImage(WebhookTesterContainerImageTags.Image)
            .WithImageRegistry(WebhookTesterContainerImageTags.Registry)
            .WithImageTag(WebhookTesterContainerImageTags.Tag)
            .WithHttpEndpoint(port: port, targetPort: DefaultContainerPort)
            .WithHttpHealthCheck("/healthz");

        return container;
    }

    public static IResourceBuilder<WebhookTesterResource> WithAutoCreateSessions(this IResourceBuilder<WebhookTesterResource> builder, bool enabled = true)
        => builder.WithEnvironment("AUTO_CREATE_SESSIONS", enabled.ToString().ToLowerInvariant());

    public static IResourceBuilder<WebhookTesterResource> WithDefaultSessionToken(this IResourceBuilder<WebhookTesterResource> builder, string token)
        => builder.WithEnvironment("DEFAULT_SESSION_TOKEN", token);

    public static IResourceBuilder<WebhookTesterResource> WithUiEnabled(this IResourceBuilder<WebhookTesterResource> builder, bool enabled = true)
        => builder.WithEnvironment("UI_ENABLED", enabled.ToString().ToLowerInvariant());

    public static IResourceBuilder<WebhookTesterResource> WithLogLevel(this IResourceBuilder<WebhookTesterResource> builder, string level)
        => builder.WithEnvironment("LOG_LEVEL", level);

    public static IResourceBuilder<WebhookTesterResource> WithStorageDriver(this IResourceBuilder<WebhookTesterResource> builder, string driver)
        => builder.WithEnvironment("STORAGE_DRIVER", driver);

    public static IResourceBuilder<WebhookTesterResource> WithStoragePath(this IResourceBuilder<WebhookTesterResource> builder, string path)
        => builder.WithEnvironment("STORAGE_PATH", path);

    public static IResourceBuilder<WebhookTesterResource> WithLogBody(this IResourceBuilder<WebhookTesterResource> builder, bool enabled = true)
        => builder.WithEnvironment("WEBHOOKS_LOG_BODY", enabled.ToString().ToLowerInvariant());

    public static IResourceBuilder<WebhookTesterResource> WithDumpHeaders(this IResourceBuilder<WebhookTesterResource> builder, bool enabled = true)
        => builder.WithEnvironment("WEBHOOKS_DUMP_HEADERS", enabled.ToString().ToLowerInvariant());

    public static IResourceBuilder<WebhookTesterResource> WithMaxBodySize(this IResourceBuilder<WebhookTesterResource> builder, int bytes)
        => builder.WithEnvironment("WEBHOOKS_MAX_BODY_SIZE", bytes.ToString());

    public static IResourceBuilder<WebhookTesterResource> WithUrlForSession(this IResourceBuilder<WebhookTesterResource> builder, string token)
        => builder.WithUrlForEndpoint("http", url =>
        {
            url.Url = $"/s/{token}";
            url.DisplayText = "Webhook UI";
        });
}