using Azure.AI.OpenAI;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using SantasList.Domain.Models;
using SantasList.Domain.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SantasList.Infrastructure.Services;

public class GiftSuggestionService : IGiftSuggestionService
{
    private readonly OpenAIClient _openAIClient;
    private readonly QueueServiceClient _queueServiceClient;
    private readonly TableServiceClient _tableServiceClient;

    public GiftSuggestionService(OpenAIClient openAIClient, QueueServiceClient queueServiceClient, TableServiceClient tableServiceClient)
    {
        _openAIClient = openAIClient;
        _queueServiceClient = queueServiceClient;
        _tableServiceClient = tableServiceClient;
    }

    public async Task<string> QueueAsync(GiftSuggestionPrompt giftSuggestionPrompt)
    {
        var queueResponse = await _queueServiceClient.CreateQueueAsync("prompts");
        var queueClient = queueResponse.Value ?? _queueServiceClient.GetQueueClient("prompts");

        var tableClient = _tableServiceClient.GetTableClient("queue");
        await tableClient.CreateIfNotExistsAsync();

        var data = BinaryData.FromObjectAsJson(giftSuggestionPrompt);
        var receipt = await queueClient.SendMessageAsync(data);
        var tableEntity = new TableEntity("1", receipt.Value.MessageId) {
            { "status", (int)QueueStatus.Queued },
            { "suggestions", "[]" },
        };
        await tableClient.AddEntityAsync(tableEntity);
        return receipt.Value.MessageId;
    }

    public async Task<GiftSuggestionStatus> GetAsync(string messageId)
    {
        var tableClient = _tableServiceClient.GetTableClient("queue");
        await tableClient.CreateIfNotExistsAsync();

        var response = await tableClient.GetEntityAsync<TableEntity>("1", messageId);
        var tableEntity = response.Value;
        var status = tableEntity.GetInt32("status");
        var suggestionsJson = tableEntity.GetString("suggestions");

        var suggestions = JsonSerializer.Deserialize<IEnumerable<GiftSuggestion>>(suggestionsJson);

        return new GiftSuggestionStatus(status.HasValue ? (QueueStatus)status.Value : QueueStatus.Queued, suggestions);
    }

    public async Task<IEnumerable<GiftSuggestion>> ProcessAsync(GiftSuggestionPrompt giftSuggestionPrompt)
    {
        ChatCompletionsOptions completionOptions = new() {
            MaxTokens = 2048,
            Temperature = 0.7f,
            NucleusSamplingFactor = 0.95f,
            DeploymentName = "gpt-35-turbo"
        };

        completionOptions.Messages.Add(new ChatMessage(ChatRole.System, "you are a helpful Christmas themed assistant and help people by suggesting gifts to purchase."));
        completionOptions.Messages.Add(new ChatMessage(ChatRole.User, giftSuggestionPrompt.ToPrompt()));

        var response = await _openAIClient.GetChatCompletionsAsync(completionOptions);

        return await ParseSuggestionsAsync(response.Value);
    }

    private async Task<IEnumerable<GiftSuggestion>> ParseSuggestionsAsync(ChatCompletions chatCompletions)
    {
        var suggestions = new List<GiftSuggestion>();
        string numberedListRegex = @"(\d\. )(.*)$";

        foreach (var item in chatCompletions.Choices) {
            var matches = Regex.Matches(item.Message.Content, numberedListRegex, RegexOptions.Multiline);

            if (matches != null) {
                for (int i = 0; i < matches.Count; i++) {
                    suggestions.Add(new GiftSuggestion(matches[i].Groups[2].Value));
                }
            }
        }

        return suggestions;
    }
}
