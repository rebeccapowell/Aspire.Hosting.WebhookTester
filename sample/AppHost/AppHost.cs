using Aspire.Hosting.WebhookTester;

var builder = DistributedApplication.CreateBuilder(args);

var token = Guid.NewGuid().ToString();

var webhookTester = builder.AddWebhookTester("webhook-tester")
	.WithAutoCreateSessions()
	.WithDefaultSessionToken(token)
	.WithUiEnabled(true)
	.WithLogLevel("debug")
	.WithLogBody()
	.WithDumpHeaders()
	.WithMaxBodySize(1048576)
	.WithUrlForSession(token);

builder.Build().Run();