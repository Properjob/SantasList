using Azure;
using Azure.AI.OpenAI;
using SantasList.ApiService.Services;
using SantasList.Domain.Services;
using SantasList.Domain.Models;

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

builder.Services.AddScoped<IGiftSuggestionService, GiftSuggestionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/suggestions", async (IGiftSuggestionService giftSuggestionService) =>
{
    var prompt = new GiftSuggestionPrompt() {
        IdentifiedGender = "Man",
        Age = 28,
        Budget = 50,
        Currency = "£",
        Interests = ["Cars", "Minecraft", "Computers", "Programming"],
    };

    return await giftSuggestionService.GetSuggestionsAsync(prompt);
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
