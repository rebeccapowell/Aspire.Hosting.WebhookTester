using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Register Aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Retrieve the webhook tester connection string
var webhookUrl = builder.Configuration.GetConnectionString("webhook-tester");
Console.WriteLine($"Webhook URL: {webhookUrl}");
var services = builder.Configuration.GetSection("Services");
Console.WriteLine($"{services.Key}: {services.Value}");
var defaultToken = builder.Configuration["DEFAULT_SESSION_TOKEN"];
Console.WriteLine($"Default token: {defaultToken}");

// Register an HttpClient named "webhook-tester"
builder.Services.AddHttpClient("webhook-tester", client =>
{
    //client.BaseAddress = new Uri(webhookUrl ?? throw new InvalidOperationException("Missing connection string for webhook-tester"));
    client.BaseAddress = new Uri("http://webhook-tester");
    // Accept only JSON responses
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
}).AddServiceDiscovery();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDefaultEndpoints();

app.MapPost("/test", async (
        [FromBody] TestRequest request,
        [FromHeader(Name = "webhookUri")] string? webhookUri,
        [FromServices] IHttpClientFactory httpClientFactory) =>
    {
        var httpClient = string.IsNullOrWhiteSpace(webhookUri) ?
            httpClientFactory.CreateClient("webhook-tester") :
            httpClientFactory.CreateClient();

        if (!string.IsNullOrWhiteSpace(webhookUri))
        {
            httpClient.BaseAddress = new Uri(webhookUri);
        }

        var response = await httpClient.PostAsJsonAsync($"/{defaultToken}", request);
        response.EnsureSuccessStatusCode();

        return Results.Ok();
    })
    .WithName("Test");

app.Run();

record TestRequest(string Name);
