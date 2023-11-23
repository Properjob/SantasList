using SantasList.Domain.Models;
using SantasList.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantasList.Adapter.OpenAI.Services;
public class GiftSuggestionService : IGiftSuggestionService
{
    public Task<IEnumerable<GiftSuggestion>> GetSuggestionsAsync()
    {
        throw new NotImplementedException();
    }
}
