using Azure;
using Azure.AI.OpenAI;
using SantasList.Domain.Services;
using SantasList.Infrastructure.Services;
using SantasList.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton(sd => {

    var azureOpenAIConfig = builder.Configuration.GetSection("AzureOpenAI");
    var githubUsername = azureOpenAIConfig["GithubUsername"];
    var proxy = azureOpenAIConfig["ProxyUrl"];
    var key = azureOpenAIConfig["Key"];

    // the full url is appended by /v1/api
    Uri proxyUrl = new(proxy + "/v1/api");

    // the full key is appended by "/YOUR-GITHUB-ALIAS"
    AzureKeyCredential token = new(key + "/" + githubUsername);

    return new OpenAIClient(proxyUrl, token);
});

builder.AddAzureQueueService("queue");
builder.AddAzureTableService("table");

builder.Services.AddSingleton<IGiftSuggestionService, GiftSuggestionService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
