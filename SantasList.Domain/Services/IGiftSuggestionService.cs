using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SantasList.Domain.Models;

namespace SantasList.Domain.Services;

public interface IGiftSuggestionService
{
    Task<string> QueueAsync(GiftSuggestionPrompt giftSuggestionPrompt);

    Task<GiftSuggestionStatus> GetAsync(string messageId);

    Task<IEnumerable<GiftSuggestion>> ProcessAsync(GiftSuggestionPrompt giftSuggestionPrompt);
}
