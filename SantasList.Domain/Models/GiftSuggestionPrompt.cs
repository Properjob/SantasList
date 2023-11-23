using System.Text;

namespace SantasList.Domain.Models;

public class GiftSuggestionPrompt
{
    public string IdentifiedGender { get; set; }

    public int Age { get; set; }

    public string Currency { get; set; }

    public int? Budget { get; set; }

    public IEnumerable<string> Interests { get; set; }

    public string ToPrompt()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("hi, please can you list gift ideas with a ");
        stringBuilder.Append(Currency);
        stringBuilder.Append(Budget);
        stringBuilder.Append(" for a ");
        stringBuilder.Append(Age);
        stringBuilder.Append(" year old ");
        stringBuilder.Append(IdentifiedGender);
        stringBuilder.Append(" who's interests are ");
        stringBuilder.Append(string.Join(", ", Interests.SkipLast(1)));
        stringBuilder.Append(" and ");
        stringBuilder.Append(Interests.Last());

        return stringBuilder.ToString();
    }
}