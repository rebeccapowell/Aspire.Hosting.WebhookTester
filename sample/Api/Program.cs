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

// Register an HttpClient named "webhook-tester"
builder.Services.AddHttpClient("webhook-tester", client =>
{
    client.BaseAddress = new Uri(webhookUrl ?? throw new InvalidOperationException("Missing connection string for webhook-tester"));
    // Accept only JSON responses
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDefaultEndpoints();

app.MapPost("/test", async (
        [FromBody] TestRequest request,
        IHttpClientFactory httpClientFactory) =>
    {
        var httpClient = httpClientFactory.CreateClient("webhook-tester");

        var response = await httpClient.PostAsJsonAsync("", request);
        response.EnsureSuccessStatusCode();

        return Results.Ok();
    })
    .WithName("Test");

app.Run();

record TestRequest(string Name);
