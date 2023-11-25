using SantasList.Domain.Models;

namespace SantasList.Web.Services;

public class GiftSuggestionApiClient(HttpClient httpClient)
{
    public async Task<string> PostAsync(GiftSuggestionForm giftSuggestionForm)
    {
        var prompt = new GiftSuggestionPrompt() {
            IdentifiedGender = giftSuggestionForm.IdentifiedGender,
            Age = giftSuggestionForm.Age,
            Budget = giftSuggestionForm.Budget,
            Currency = giftSuggestionForm.Currency,
            Interests = new string[] { giftSuggestionForm.Interest1, giftSuggestionForm.Interest2, giftSuggestionForm.Interest3, giftSuggestionForm.Interest4, }
        };

        var response = await httpClient.PostAsJsonAsync("/suggestions/queue", prompt);

        return response.Headers.Location.ToString();
    }

    public async Task<GiftSuggestionStatus> GetAsync(string messageId)
    {
        return await httpClient.GetFromJsonAsync<GiftSuggestionStatus>($"/suggestions/{messageId}");
    }
}
