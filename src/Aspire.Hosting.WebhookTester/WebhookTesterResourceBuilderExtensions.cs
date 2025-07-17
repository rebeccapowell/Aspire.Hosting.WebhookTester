using System.Globalization;
using Aspire.Hosting.ApplicationModel;

// ReSharper disable once CheckNamespace
namespace Aspire.Hosting;

/// <summary>
/// Storage Driver types.
/// </summary>
public enum StorageDriver
{
    /// <summary>
    /// In-Memory driver.
    /// </summary>
    Memory,
    /// <summary>
    /// Disk driver.
    /// </summary>
    Disk
}

/// <summary>
/// Pub/Sub driver types.
/// </summary>
public enum PubSubDriver
{
    /// <summary>
    /// In-memory driver.
    /// </summary>
    Memory,
    /// <summary>
    /// Redis driver.
    /// </summary>
    Redis
}

/// <summary>
/// Tunnel driver types.
/// </summary>
public enum TunnelDriver
{
    /// <summary>
    /// Ngrok tunnel driver (requires auth token).
    /// </summary>
    Ngrok
}

/// <summary>
/// Log level options.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Debug level logging.
    /// </summary>
    Debug,
    /// <summary>
    /// Informational level logging.
    /// </summary>
    Info,
    /// <summary>
    /// Warning level logging.
    /// </summary>
    Warn,
    /// <summary>
    /// Error level logging.
    /// </summary>
    Error
}

/// <summary>
/// Log format options.
/// </summary>
public enum LogFormat
{
    /// <summary>
    /// Plain text log format.
    /// </summary>
    Text,
    /// <summary>
    /// JSON structured log format.
    /// </summary>
    Json
}

/// <summary>
/// Fluent extensions for configuring the Webhook Tester container resource.
/// </summary>
public static class WebhookTesterResourceBuilderExtensions
{
    /// <summary>
    /// Formats a TimeSpan into a Go-compatible duration string (e.g., "15s" or "1500ms").
    /// </summary>
    private static string ToGoDuration(TimeSpan ts)
    {
        // If whole seconds, use seconds
        if (ts.Ticks % TimeSpan.TicksPerSecond == 0)
        {
            return $"{(long)ts.TotalSeconds}s";
        }
        // Otherwise, use milliseconds
        return $"{(long)ts.TotalMilliseconds}ms";
    }
    
    private const int DefaultContainerPort = 8080;

    /// <summary>
    /// Adds a Webhook Tester container to the application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">Resource name.</param>
    /// <param name="token">Optional session token; if null, a new GUID will be generated.</param>
    /// <param name="autoCreateSessions">Whether to auto-create a default session.</param>
    /// <param name="port">Override the default port.</param>
    public static IResourceBuilder<WebhookTesterResource> AddWebhookTester(
        this IDistributedApplicationBuilder builder,
        string name,
        string? token = null,
        bool autoCreateSessions = true,
        int? port = null)
    {
        token ??= Guid.NewGuid().ToString();
        var resource = new WebhookTesterResource(name, token);

        var container = builder.AddResource(resource)
            .WithImage(WebhookTesterContainerImageTags.Image)
            .WithImageRegistry(WebhookTesterContainerImageTags.Registry)
            .WithImageTag(WebhookTesterContainerImageTags.Tag)
            .WithHttpEndpoint(port: port, targetPort: DefaultContainerPort)
            .WithHttpHealthCheck("/healthz")
            .WithEnvironment("DEFAULT_SESSION_TOKEN", token);

        if (port.HasValue)
        {
            container = container.WithEnvironment(
                "HTTP_PORT",
                port.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (autoCreateSessions)
        {
            container = container.WithEnvironment("AUTO_CREATE_SESSIONS", "true");
        }

        container = container.WithUrlForEndpoint("http", u =>
        {
            u.Url = $"/s/{token}";
            u.DisplayText = "Webhook (UI)";
        });
        
        container = container.WithUrlForEndpoint("https", u =>
        {
            u.Url = $"/s/{token}";
            u.DisplayText = "Webhook (UI)";
        });

        return container;
    }

    /// <summary>
    /// Sets the HTTP port (container listen port).
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithHttpPort(
        this IResourceBuilder<WebhookTesterResource> builder,
        int port)
        => builder.WithEnvironment(
            "HTTP_PORT",
            port.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// Enables or disables auto session creation.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithAutoCreateSessions(
        this IResourceBuilder<WebhookTesterResource> builder,
        bool enabled = true)
        => builder.WithEnvironment("AUTO_CREATE_SESSIONS", enabled.ToString().ToLowerInvariant());

    /// <summary>
    /// Sets the log level.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithLogLevel(
        this IResourceBuilder<WebhookTesterResource> builder,
        LogLevel level)
        => builder.WithEnvironment(
            "LOG_LEVEL",
            level.ToString().ToLowerInvariant());

    /// <summary>
    /// Sets the log format.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithLogFormat(
        this IResourceBuilder<WebhookTesterResource> builder,
        LogFormat format)
        => builder.WithEnvironment(
            "LOG_FORMAT",
            format.ToString().ToLowerInvariant());

    /// <summary>
    /// Sets the server listen address.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithServerAddress(
        this IResourceBuilder<WebhookTesterResource> builder,
        string address)
        => builder.WithEnvironment("SERVER_ADDR", address);

    /// <summary>
    /// Sets the HTTP read timeout.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithHttpReadTimeout(
        this IResourceBuilder<WebhookTesterResource> builder,
        TimeSpan timeout)
        => builder.WithEnvironment("HTTP_READ_TIMEOUT", ToGoDuration(timeout));

    /// <summary>
    /// Sets the HTTP write timeout.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithHttpWriteTimeout(
        this IResourceBuilder<WebhookTesterResource> builder,
        TimeSpan timeout)
        => builder.WithEnvironment("HTTP_WRITE_TIMEOUT", ToGoDuration(timeout));

    /// <summary>
    /// Sets the HTTP idle timeout.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithHttpIdleTimeout(
        this IResourceBuilder<WebhookTesterResource> builder,
        TimeSpan timeout)
        => builder.WithEnvironment("HTTP_IDLE_TIMEOUT", ToGoDuration(timeout));

    /// <summary>
    /// Sets the shutdown timeout.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithShutdownTimeout(
        this IResourceBuilder<WebhookTesterResource> builder,
        TimeSpan timeout)
        => builder.WithEnvironment("SHUTDOWN_TIMEOUT", ToGoDuration(timeout));

    /// <summary>
    /// Sets the storage driver.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithStorageDriver(
        this IResourceBuilder<WebhookTesterResource> builder,
        StorageDriver driver)
        => builder.WithEnvironment(
            "STORAGE_DRIVER",
            driver.ToString().ToLowerInvariant());

    /// <summary>
    /// Sets the filesystem storage directory.
    /// Path to the directory for local fs storage (directory must exist).
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithFsStorageDir(
        this IResourceBuilder<WebhookTesterResource> builder,
        string dir)
        => builder.WithEnvironment("FS_STORAGE_DIR", dir);

    /// <summary>
    /// Sets the pubsub driver.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithPubSubDriver(
        this IResourceBuilder<WebhookTesterResource> builder,
        PubSubDriver driver)
        => builder.WithEnvironment(
            "PUBSUB_DRIVER",
            driver.ToString().ToLowerInvariant());

    /// <summary>
    /// Sets the tunnel driver (ngrok).
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithTunnelDriver(
        this IResourceBuilder<WebhookTesterResource> builder,
        TunnelDriver driver)
    {
        if (driver == TunnelDriver.Ngrok)
        {
            builder = builder.WithEnvironment("TUNNEL_DRIVER", "ngrok");
        }
        return builder;
    }

    /// <summary>
    /// Sets the Ngrok auth token.
    /// ngrok authentication token (required for ngrok tunnel; create a new one
    /// at https://dashboard.ngrok.com/authtokens/new)
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithNgrokAuthToken(
        this IResourceBuilder<WebhookTesterResource> builder,
        string token)
        => builder.WithEnvironment("NGROK_AUTHTOKEN", token);

    /// <summary>
    /// Sets the Redis DSN for pubsub driver. Must be a valid URI with 'redis' or 'unix' scheme.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithRedisDsn(
        this IResourceBuilder<WebhookTesterResource> builder,
        string dsn)
    {
        // Validate DSN format: allow redis:// or unix://
        if (!Uri.TryCreate(dsn, UriKind.Absolute, out var uri) ||
            ( !string.Equals(uri.Scheme, "redis", StringComparison.OrdinalIgnoreCase)
              && !string.Equals(uri.Scheme, "unix", StringComparison.OrdinalIgnoreCase) ))
        {
            throw new ArgumentException(
                "Invalid Redis DSN. Expected a URI with scheme 'redis://' or 'unix://', e.g. 'redis://user:pwd@127.0.0.1:6379/0' or 'unix://user:pwd@/path/to/socket?db=0'.",
                nameof(dsn));
        }
        return builder.WithEnvironment("REDIS_DSN", dsn);
    }

    /// <summary>
    /// Sets the session TTL limit.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithSessionTtl(
        this IResourceBuilder<WebhookTesterResource> builder,
        TimeSpan ttl)
        => builder.WithEnvironment("SESSION_TTL", ttl.ToString());

    /// <summary>
    /// Sets the maximum number of sessions.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithMaxRequests(
        this IResourceBuilder<WebhookTesterResource> builder,
        uint max)
        => builder.WithEnvironment(
            "MAX_REQUESTS",
            max.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// Sets the maximum request body size.
    /// </summary>
    public static IResourceBuilder<WebhookTesterResource> WithMaxRequestBodySize(
        this IResourceBuilder<WebhookTesterResource> builder,
        uint bytes)
        => builder.WithEnvironment(
            "MAX_REQUEST_BODY_SIZE",
            bytes.ToString(CultureInfo.InvariantCulture));
    
    /// <summary>
    /// Adds a reference to the Webhook Tester and injects its environment variables into the consuming resource.
    /// </summary>
    /// <typeparam name="T">The type of the consuming resource.</typeparam>
    /// <param name="builder">The resource builder for the consumer.</param>
    /// <param name="webhook">The webhook tester resource.</param>
    public static IResourceBuilder<T> WithDefaultWebhookToken<T>(
        this IResourceBuilder<T> builder,
        IResourceBuilder<WebhookTesterResource> webhook)
        where T : IResourceWithEnvironment
    {
        return builder
            .WithEnvironment(ctx =>
            {
                var token = webhook.Resource.EnvironmentVariables["DEFAULT_SESSION_TOKEN"];
                ctx.EnvironmentVariables.Add("DEFAULT_SESSION_TOKEN", token!);
            });
    }
}
