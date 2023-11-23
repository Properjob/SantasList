using Azure.AI.OpenAI;
using SantasList.Domain.Models;
using SantasList.Domain.Services;
using System.Text.RegularExpressions;

namespace SantasList.ApiService.Services;

public class GiftSuggestionService : IGiftSuggestionService
{
    private readonly OpenAIClient _openAIClient;

    public GiftSuggestionService(OpenAIClient openAIClient)
    {
        _openAIClient = openAIClient;
    }

    public async Task<IEnumerable<GiftSuggestion>> GetSuggestionsAsync(GiftSuggestionPrompt giftSuggestionPrompt)
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
