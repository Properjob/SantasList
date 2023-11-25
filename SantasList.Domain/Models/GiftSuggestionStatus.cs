using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantasList.Domain.Models;

public enum QueueStatus
{
    Queued,
    Processing,
    Processed,
}

public record GiftSuggestionStatus(QueueStatus Status, IEnumerable<GiftSuggestion> GiftSuggestions);
