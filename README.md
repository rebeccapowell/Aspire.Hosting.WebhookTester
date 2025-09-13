# Aspire.Hosting.WebhookTester

**Webhook Tester** is a small [Aspire](https://github.com/dotnet/aspire) extension that deploys the [`tarampampam/webhook-tester`](https://hub.docker.com/r/tarampampam/webhook-tester) container. It provides a simple way to inspect HTTP requests from your application while running locally.

This repository contains the extension library, a sample app that shows how to use it and a small test suite.

## Installation

Add the NuGet package to your AppHost project:

```bash
dotnet add package Allesa.Aspire.Hosting.WebhookTester
```

The extension targets **.NET 9** and ships as a regular NuGet package, so no additional tooling is required.

## Basic usage

Create a distributed application and add the webhook tester container:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var webhook = builder.AddWebhookTester("webhook-tester")
                     .WithLogLevel(LogLevel.Debug);

var api = builder.AddProject<Projects.Api>("api")
                 .WithReference(webhook)
                 .WithDefaultWebhookToken(webhook);

builder.Build().Run();
```

Calling `AddWebhookTester` registers the container and exposes two URLs (HTTP/HTTPS) ending with `/s/{token}` for the Web UI. The connection string for other services is `http://{host}:{port}/{token}` and can be retrieved via `GetConnectionString("webhook-tester")`.

## Extension methods

`WebhookTesterResourceBuilderExtensions` exposes a rich set of configuration helpers. Each method sets an environment variable understood by the container:

| Method | Environment variable | Description |
|-------|---------------------|-------------|
| `WithHttpPort(int)` | `HTTP_PORT` | Override the listening port (defaults to 8080). |
| `WithAutoCreateSessions(bool)` | `AUTO_CREATE_SESSIONS` | Automatically create an initial session. |
| `WithLogLevel(LogLevel)` | `LOG_LEVEL` | Logging level (`debug`, `info`, `warn`, `error`). |
| `WithLogFormat(LogFormat)` | `LOG_FORMAT` | Log output format (`text` or `json`). |
| `WithServerAddress(string)` | `SERVER_ADDR` | Custom listen address. |
| `WithHttpReadTimeout(TimeSpan)` | `HTTP_READ_TIMEOUT` | HTTP server read timeout. |
| `WithHttpWriteTimeout(TimeSpan)` | `HTTP_WRITE_TIMEOUT` | HTTP server write timeout. |
| `WithHttpIdleTimeout(TimeSpan)` | `HTTP_IDLE_TIMEOUT` | HTTP server idle timeout. |
| `WithShutdownTimeout(TimeSpan)` | `SHUTDOWN_TIMEOUT` | Graceful shutdown timeout. |
| `WithStorageDriver(StorageDriver)` | `STORAGE_DRIVER` | Storage backend (`memory` or `disk`). |
| `WithFsStorageDir(string)` | `FS_STORAGE_DIR` | Directory for disk storage. |
| `WithPubSubDriver(PubSubDriver)` | `PUBSUB_DRIVER` | Pub/Sub backend (`memory` or `redis`). |
| `WithRedisDsn(string)` | `REDIS_DSN` | Redis connection string when using the Redis pub/sub driver. |
| `WithTunnelDriver(TunnelDriver)` | `TUNNEL_DRIVER` | Tunnel provider (`ngrok`). |
| `WithNgrokAuthToken(string)` | `NGROK_AUTHTOKEN` | ngrok authentication token. |
| `WithSessionTtl(TimeSpan)` | `SESSION_TTL` | Session expiration duration. |
| `WithMaxRequests(uint)` | `MAX_REQUESTS` | Maximum number of requests per session. |
| `WithMaxRequestBodySize(uint)` | `MAX_REQUEST_BODY_SIZE` | Limit request payload size in bytes. |
| `WithDefaultWebhookToken<T>` | `DEFAULT_SESSION_TOKEN` | Injects the default token into another resource. |

`AddWebhookTester` itself sets `DEFAULT_SESSION_TOKEN` (always), `AUTO_CREATE_SESSIONS` (when enabled) and `HTTP_PORT` (when a custom port is provided).

## Sample application

The `sample` directory contains a minimal Aspire solution:

- **AppHost** – registers the webhook tester and an API project.
- **Api** – exposes a `/test` endpoint which posts data to the tester.
- **ServiceDefaults** – shared hosting configuration used by the API.

Run it with:

```bash
dotnet run --project sample/AppHost
```

During startup the AppHost prints the generated webhook URL. Sending a POST request to `Api` will forward it to the tester. You can then browse to the tester's Web UI at `/s/{token}` to inspect captured requests.

## Tests

The `tests` folder contains unit tests for the builder API. Integration tests that spin up containers require Docker and are located under `sample/SampleTests`. When running locally you can execute just the builder tests:

```bash
dotnet test tests/Aspire.Hosting.WebhookTester.Tests --filter FullyQualifiedName~WebhookTesterPublicApiTests
```

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for details. 
