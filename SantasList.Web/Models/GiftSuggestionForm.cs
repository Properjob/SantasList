using System.Text;

namespace SantasList.Domain.Models;

public class GiftSuggestionForm
{
    public string IdentifiedGender { get; set; }

    public int Age { get; set; }

    public string Currency { get; set; }

    public int? Budget { get; set; }

    public string Interest1 { get; set; }

    public string Interest2 { get; set; }

    public string Interest3 { get; set; }

    public string Interest4 { get; set; }
}