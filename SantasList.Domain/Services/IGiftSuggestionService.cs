using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SantasList.Domain.Models;

namespace SantasList.Domain.Services;

public interface IGiftSuggestionService
{
    Task<IEnumerable<GiftSuggestion>> GetSuggestionsAsync(GiftSuggestionPrompt giftSuggestionPrompt);
}
