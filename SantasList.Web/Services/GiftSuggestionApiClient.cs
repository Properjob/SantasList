using SantasList.Domain.Models;

namespace SantasList.Web.Services;

public class GiftSuggestionApiClient(HttpClient httpClient)
{
    public async Task<GiftSuggestion[]> GetSuggestionsAsync()
    {
        return await httpClient.GetFromJsonAsync<GiftSuggestion[]>("/suggestions") ?? [];
    }
}
