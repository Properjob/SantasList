using Azure;
using Azure.AI.OpenAI;
using SantasList.Domain.Services;
using SantasList.Domain.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Reflection.PortableExecutable;
using Azure.Data.Tables;
using SantasList.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddScoped(sd => {

    var azureOpenAIConfig = builder.Configuration.GetSection("AzureOpenAI");
    var githubUsername = azureOpenAIConfig["GithubUsername"];
    var proxy= azureOpenAIConfig["ProxyUrl"];
    var key = azureOpenAIConfig["Key"];

    // the full url is appended by /v1/api
    Uri proxyUrl = new(proxy + "/v1/api");

    // the full key is appended by "/YOUR-GITHUB-ALIAS"
    AzureKeyCredential token = new(key + "/" + githubUsername);

    return new OpenAIClient(proxyUrl, token);
});


builder.AddAzureQueueService("queue");
builder.AddAzureTableService("table");

builder.Services.AddScoped<IGiftSuggestionService, GiftSuggestionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapPost("/suggestions/queue", async (IGiftSuggestionService giftSuggestionService, [FromBody] GiftSuggestionPrompt giftSuggestionPrompt) => {
    var messageId = await giftSuggestionService.QueueAsync(giftSuggestionPrompt);
    return Results.Accepted($"/suggestions/{messageId}");
});

app.MapGet("/suggestions/{messageId}", async (IGiftSuggestionService giftSuggestionService, string messageId) =>
{
    var status = await giftSuggestionService.GetAsync(messageId);
    return Results.Ok(status);
});

app.MapDefaultEndpoints();

app.Run();
