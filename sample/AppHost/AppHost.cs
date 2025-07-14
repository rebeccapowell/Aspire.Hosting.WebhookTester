var builder = DistributedApplication.CreateBuilder(args);

var token = Guid.NewGuid().ToString();

var webhookTester = builder.AddWebhookTester("webhook-tester", token)
    .WithLogLevel(LogLevel.Debug);

var api = builder
    .AddProject<Projects.Api>("api")
    .WithReference(webhookTester);

builder.Build().Run();