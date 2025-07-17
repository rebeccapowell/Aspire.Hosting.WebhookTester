# WebhookTester Aspire Extension Specification

This document reverse engineers the behaviour of the `Powell.Aspire.Hosting.WebhookTester` package. It can be used as a specification for implementing a compatible extension from scratch.

## Project structure

* Target framework: **.NET&nbsp;9**
* Package ID: `Powell.Aspire.Hosting.WebhookTester`
* Contains a single class library with file-scoped namespaces and implicit `using` directives.
* Public APIs are defined inside the `Aspire.Hosting` namespace.

## Container image

The extension launches the [tarampampam/webhook-tester](https://hub.docker.com/r/tarampampam/webhook-tester) container:

```csharp
internal static class WebhookTesterContainerImageTags
{
    public const string Registry = "ghcr.io";
    public const string Image = "tarampampam/webhook-tester";
    public const string Tag = "2"; // or a specific version
}
```

## Resource implementation

`WebhookTesterResource` derives from `ContainerResource` and implements `IResourceWithConnectionString` and `IResourceWithServiceDiscovery`.

```csharp
public sealed class WebhookTesterResource : ContainerResource,
    IResourceWithConnectionString, IResourceWithServiceDiscovery
```

Constructor parameters:

```csharp
WebhookTesterResource(string name, string defaultSessionToken)
```

* Stores `DefaultSessionToken`.
* `PrimaryEndpoint` is lazily created with name `"http"`.
* `EnvironmentVariables` exposes `{ "DEFAULT_SESSION_TOKEN", DefaultSessionToken }` for injection into other projects.
* `ConnectionStringExpression` builds `http://{host}:{port}/{token}` from the primary endpoint and `DefaultSessionToken`.

`GetConnectionStringAsync` honours `ConnectionStringRedirectAnnotation` if present.

## Builder extensions

The static class `WebhookTesterResourceBuilderExtensions` defines the fluent API. Important points:

### AddWebhookTester

```csharp
IResourceBuilder<WebhookTesterResource> AddWebhookTester(
    this IDistributedApplicationBuilder builder,
    string name,
    string? token = null,
    bool autoCreateSessions = true,
    int? port = null)
```

* Throws `ArgumentNullException` when `builder` or `name` are null.
* When `token` is `null`, a new GUID is generated.
* Registers a `WebhookTesterResource` and sets the container image based on `WebhookTesterContainerImageTags`.
* Exposes an HTTP endpoint on `port` (defaults to **8080** inside the container).
* Adds a health check on `/healthz`.
* Sets environment variables:
  * `DEFAULT_SESSION_TOKEN` – always present with the chosen token.
  * `HTTP_PORT` – only when a custom port was provided.
  * `AUTO_CREATE_SESSIONS` – when `autoCreateSessions` is `true`.
* Adds UI links for the HTTP and HTTPS endpoints with path `/s/{token}`.

### Configuration helpers

Each helper validates its parameters and then calls `WithEnvironment` on the underlying `IResourceBuilder<WebhookTesterResource>`:

* `WithHttpPort(int)` → `HTTP_PORT`
* `WithAutoCreateSessions(bool)` → `AUTO_CREATE_SESSIONS`
* `WithLogLevel(LogLevel)` → `LOG_LEVEL`
* `WithLogFormat(LogFormat)` → `LOG_FORMAT`
* `WithServerAddress(string)` → `SERVER_ADDR`
* `WithHttpReadTimeout(TimeSpan)` → `HTTP_READ_TIMEOUT` (`15s`, `1500ms` etc.)
* `WithHttpWriteTimeout(TimeSpan)` → `HTTP_WRITE_TIMEOUT`
* `WithHttpIdleTimeout(TimeSpan)` → `HTTP_IDLE_TIMEOUT`
* `WithShutdownTimeout(TimeSpan)` → `SHUTDOWN_TIMEOUT`
* `WithStorageDriver(StorageDriver)` → `STORAGE_DRIVER`
* `WithFsStorageDir(string)` → `FS_STORAGE_DIR`
* `WithPubSubDriver(PubSubDriver)` → `PUBSUB_DRIVER`
* `WithTunnelDriver(TunnelDriver)` → `TUNNEL_DRIVER` (only value is `ngrok`)
* `WithNgrokAuthToken(string)` → `NGROK_AUTHTOKEN`
* `WithRedisDsn(string)` → `REDIS_DSN` – validates the URI scheme is `redis` or `unix`.
* `WithSessionTtl(TimeSpan)` → `SESSION_TTL`
* `WithMaxRequests(uint)` → `MAX_REQUESTS`
* `WithMaxRequestBodySize(uint)` → `MAX_REQUEST_BODY_SIZE`

### WithDefaultWebhookToken

```csharp
IResourceBuilder<T> WithDefaultWebhookToken<T>(
    this IResourceBuilder<T> builder,
    IResourceBuilder<WebhookTesterResource> webhook)
    where T : IResourceWithEnvironment
```

Copies `DEFAULT_SESSION_TOKEN` from the webhook tester resource into the consuming resource's environment variables.

## Enumerations

The API exposes several enums used by the configuration helpers:

* `StorageDriver` – `Memory`, `Disk`
* `PubSubDriver` – `Memory`, `Redis`
* `TunnelDriver` – `Ngrok`
* `LogLevel` – `Debug`, `Info`, `Warn`, `Error`
* `LogFormat` – `Text`, `Json`

## Expected behaviour

An agent implementing this specification should produce an extension whose public surface matches the APIs above and that sets the correct environment variables when used in an Aspire AppHost project. Unit tests similar to those under `tests/Aspire.Hosting.WebhookTester.Tests` should pass against the implementation.

